using Carter;
using Fei.Is.Api.Data.Contexts;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Queries;

public static class GetTimescaleDataPointChunks
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "system/storage/chunks",
                    async Task<Ok<IReadOnlyCollection<Response>>> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Query(), cancellationToken);
                        return TypedResults.Ok(result);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(GetTimescaleDataPointChunks))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Get TimescaleDB datapoint chunks";
                    o.Description = "Returns datapoint chunk metadata including time ranges and storage sizes.";
                    return o;
                });
        }
    }

    public record Query() : IRequest<IReadOnlyCollection<Response>>;

    public record Response(
        string ChunkSchema,
        string ChunkName,
        DateTimeOffset? RangeStart,
        DateTimeOffset? RangeEnd,
        bool IsCompressed,
        long TotalBytes,
        long TableBytes,
        long IndexBytes,
        long ToastBytes
    );

    public sealed class Handler(TimeScaleDbContext timescaleContext) : IRequestHandler<Query, IReadOnlyCollection<Response>>
    {
        private const string Sql = """
            SELECT
                c.chunk_schema AS "ChunkSchema",
                c.chunk_name AS "ChunkName",
                c.range_start AS "RangeStart",
                c.range_end AS "RangeEnd",
                c.is_compressed AS "IsCompressed",
                COALESCE(s.total_bytes, 0)::bigint AS "TotalBytes",
                COALESCE(s.table_bytes, 0)::bigint AS "TableBytes",
                COALESCE(s.index_bytes, 0)::bigint AS "IndexBytes",
                COALESCE(s.toast_bytes, 0)::bigint AS "ToastBytes"
            FROM timescaledb_information.chunks c
            LEFT JOIN chunks_detailed_size('"DataPoints"'::regclass) s
                ON s.chunk_schema = c.chunk_schema AND s.chunk_name = c.chunk_name
            WHERE c.hypertable_schema = 'public' AND c.hypertable_name = 'DataPoints'
            ORDER BY c.range_start ASC NULLS LAST, c.chunk_name ASC;
            """;

        public async Task<IReadOnlyCollection<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rows = await timescaleContext.Database.SqlQueryRaw<Response>(Sql).ToListAsync(cancellationToken);
            return rows;
        }
    }
}
