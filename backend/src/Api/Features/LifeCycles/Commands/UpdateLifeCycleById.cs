using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.LifeCycles.Commands;

public static class UpdateLifeCycleById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "lifecycles/plant/{plantId:guid}",
                    async Task<Results<NoContent, NotFound>> (IMediator mediator, Guid plantId, Request request) =>
                    {
                        var command = new Command(plantId, request);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName("UpdateLifeCycleByPlantId")
                .WithTags(nameof(PlantAnalysis))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update plant analysis by PlantId";
                    return o;
                });
        }
    }

    public record Command(Guid PlantId, Request Data) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var plantAnalysis = await context.PlantAnalyses.FirstOrDefaultAsync(pa => pa.Id == request.PlantId, cancellationToken);

            if (plantAnalysis == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Update fields
            plantAnalysis.LeafCount = request.Data.LeafCount ?? plantAnalysis.LeafCount;
            plantAnalysis.Width = request.Data.Width ?? plantAnalysis.Width;
            plantAnalysis.Height = request.Data.Height ?? plantAnalysis.Height;
            plantAnalysis.Area = request.Data.Area ?? plantAnalysis.Area;
            plantAnalysis.Disease = request.Data.Disease ?? plantAnalysis.Disease;
            plantAnalysis.Health = request.Data.Health ?? plantAnalysis.Health;
            plantAnalysis.AnalysisDate = request.Data.AnalysisDate ?? plantAnalysis.AnalysisDate;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public record Request(int? LeafCount, double? Width, double? Height, double? Area, string? Disease, string? Health, DateTime? AnalysisDate);
}
