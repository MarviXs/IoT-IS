using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Fei.Is.Api.Features.System.Commands;

public static class DeleteTimescaleDataPointChunk
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "system/storage/chunks/{chunkSchema}/{chunkName}",
                    async Task<Results<Ok<Response>, NotFound, ValidationProblem>> (
                        IMediator mediator,
                        string chunkSchema,
                        string chunkName,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var result = await mediator.Send(new Command(chunkSchema, chunkName), cancellationToken);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(DeleteTimescaleDataPointChunk))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a TimescaleDB datapoint chunk";
                    o.Description = "Deletes a single datapoint chunk selected by schema and chunk name.";
                    return o;
                });
        }
    }

    public record Command(string ChunkSchema, string ChunkName) : IRequest<Result<Response>>;

    public record Response(string ChunkSchema, string ChunkName, int DroppedChunkCount, IReadOnlyCollection<string> DroppedChunks);

    public sealed class Handler(TimeScaleDbContext timescaleContext) : IRequestHandler<Command, Result<Response>>
    {
        private const string GetChunkSql = """
            SELECT
                c.range_start AS "RangeStart",
                c.range_end AS "RangeEnd"
            FROM timescaledb_information.chunks c
            WHERE c.hypertable_schema = 'public'
              AND c.hypertable_name = 'DataPoints'
              AND c.chunk_schema = @chunk_schema
              AND c.chunk_name = @chunk_name
            LIMIT 1
            """;

        private const string GetRangeChunkCountSql = """
            SELECT COUNT(*) AS "Value"
            FROM timescaledb_information.chunks c
            WHERE c.hypertable_schema = 'public'
              AND c.hypertable_name = 'DataPoints'
              AND c.range_start = @range_start
              AND c.range_end = @range_end
            """;

        private const string DropChunkByRangeSql = """
            SELECT drop_chunks(relation => '"DataPoints"', older_than => @older_than, newer_than => @newer_than) AS "Value"
            """;

        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var chunk = await LoadChunkAsync(message.ChunkSchema, message.ChunkName, cancellationToken);
            if (chunk == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!chunk.RangeStart.HasValue || !chunk.RangeEnd.HasValue)
            {
                return Result.Fail(new ValidationError(nameof(Command), "Chunk does not have a bounded time range."));
            }

            var sameRangeChunks = await CountChunksInSameRangeAsync(chunk.RangeStart.Value, chunk.RangeEnd.Value, cancellationToken);
            if (sameRangeChunks > 1)
            {
                return Result.Fail(new ValidationError(nameof(Command), "Chunk cannot be uniquely deleted because multiple chunks share the same time range."));
            }

            var droppedChunks = await DropChunkAsync(chunk.RangeStart.Value, chunk.RangeEnd.Value, cancellationToken);

            return Result.Ok(new Response(message.ChunkSchema, message.ChunkName, droppedChunks.Count, droppedChunks));
        }

        private async Task<ChunkWindowRow?> LoadChunkAsync(string chunkSchema, string chunkName, CancellationToken cancellationToken)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("chunk_schema", NpgsqlDbType.Text) { Value = chunkSchema },
                new NpgsqlParameter("chunk_name", NpgsqlDbType.Text) { Value = chunkName }
            };
            var rows = await timescaleContext.Database.SqlQueryRaw<ChunkWindowRow>(GetChunkSql, parameters).ToListAsync(cancellationToken);
            return rows.FirstOrDefault();
        }

        private async Task<int> CountChunksInSameRangeAsync(DateTimeOffset rangeStart, DateTimeOffset rangeEnd, CancellationToken cancellationToken)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("range_start", NpgsqlDbType.TimestampTz) { Value = rangeStart },
                new NpgsqlParameter("range_end", NpgsqlDbType.TimestampTz) { Value = rangeEnd }
            };
            var rows = await timescaleContext.Database.SqlQueryRaw<int>(GetRangeChunkCountSql, parameters).ToListAsync(cancellationToken);
            return rows.Single();
        }

        private async Task<List<string>> DropChunkAsync(DateTimeOffset rangeStart, DateTimeOffset rangeEnd, CancellationToken cancellationToken)
        {
            var parameters = new[]
            {
                new NpgsqlParameter("older_than", NpgsqlDbType.TimestampTz) { Value = rangeEnd },
                new NpgsqlParameter("newer_than", NpgsqlDbType.TimestampTz) { Value = rangeStart }
            };
            return await timescaleContext.Database.SqlQueryRaw<string>(DropChunkByRangeSql, parameters).ToListAsync(cancellationToken);
        }

        public record ChunkWindowRow(DateTimeOffset? RangeStart, DateTimeOffset? RangeEnd);
    }
}
