using Carter;
using Fei.Is.Api.Data.Contexts;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Queries;

public static class GetTimescaleStorageUsage
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "system/storage",
                    async Task<Ok<Response>> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Query(), cancellationToken);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetTimescaleStorageUsage))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Get TimescaleDB columnstore storage usage";
                    o.Description = "Retrieve the total size of compressed data stored for datapoints in TimescaleDB.";
                    return o;
                });
        }
    }

    public record Query() : IRequest<Response>;

    public sealed class Handler(TimeScaleDbContext timescaleContext) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    COALESCE(SUM(s.after_compression_total_bytes), 0)::bigint AS "Value"
                FROM timescaledb_information.hypertable_columnstore_settings AS h
                CROSS JOIN LATERAL public.hypertable_columnstore_stats(h.hypertable::regclass) AS s
                WHERE s.number_compressed_chunks > 0
                """;

            var totalSize = await timescaleContext.Database.SqlQueryRaw<long>(sql).SingleAsync(cancellationToken);

            return new Response(totalSize);
        }
    }

    public record Response(long TotalColumnstoreSizeBytes);
}
