using Carter;
using Fei.Is.Api.Data.Contexts;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Fei.Is.Api.Features.System.Commands;

public static class VacuumTimescaleDatapoints
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/storage/vacuum",
                    async Task<Ok> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        await mediator.Send(new Command(), cancellationToken);
                        return TypedResults.Ok();
                    }
                )
                .WithName(nameof(VacuumTimescaleDatapoints))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Run VACUUM on the datapoints hypertable";
                    o.Description = "Triggers VACUUM (ANALYZE) on the TimescaleDB datapoints hypertable to reclaim storage.";
                    return o;
                });
        }
    }

    public record Command() : IRequest<Unit>;

    public sealed class Handler(TimeScaleDbContext timescaleContext) : IRequestHandler<Command, Unit>
    {
        private const string VacuumSql = "VACUUM (ANALYZE) \"DataPoints\"";

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await timescaleContext.Database.ExecuteSqlRawAsync(VacuumSql, cancellationToken);
            return Unit.Value;
        }
    }
}
