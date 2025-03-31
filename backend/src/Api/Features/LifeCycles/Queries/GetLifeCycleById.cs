using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.LifeCycles.Queries;

public static class GetLifeCycleById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "lifecycles/plant/{plantId:Guid}",
                async Task<Ok<List<Response>>> (AppDbContext context, Guid plantId) =>
                {
                    var plantAnalyses = await context
                    .PlantAnalyses.AsNoTracking()
                    .Where(pa => pa.PlantId == plantId)
                    .ToListAsync();

                    var plants = await context
                    .Plants.AsNoTracking()
                    .Where(p => p.Id == plantId)
                    .ToListAsync();

                var resolvedPlantId = "";

                    if (!plants.Any())
                    {
                        resolvedPlantId = "PlantID not found";
                    }else{
                        resolvedPlantId = plants.First().PlantId;
                    }

                var responses = plantAnalyses.Select(pa => new Response(
                    PlantId: resolvedPlantId,
                    LeafCount: pa.LeafCount,
                    width: pa.Width,
                    height: pa.Height,
                    area: pa.Area,
                    Disease: pa.Disease,
                    Health: pa.Health,
                    AnalysisDate: pa.AnalysisDate,
                    ImageName: pa.ImageName
                )).ToList();

                return TypedResults.Ok(responses);
                }
            )
            .WithName("GetLifeCyclesByPlantId")
            .WithTags(nameof(PlantAnalysis))
            .WithOpenApi(o =>
            {
                o.Summary = "Get all lifecycles by PlantId";
                return o;
            });
        }
    }

    public record Query(Guid Id) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var plantAnalysis = await context
                .PlantAnalyses.AsNoTracking()
                .FirstOrDefaultAsync(plantAnalysis => plantAnalysis.Id == request.Id, cancellationToken);

            if (plantAnalysis == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var response = new Response(
                PlantId: plantAnalysis.PlantId.ToString(),
                LeafCount: plantAnalysis.LeafCount,
                width: plantAnalysis.Width,
                height: plantAnalysis.Height,
                area: plantAnalysis.Area,
                Disease: plantAnalysis.Disease,
                Health: plantAnalysis.Health,
                AnalysisDate: plantAnalysis.AnalysisDate,
                ImageName: plantAnalysis.ImageName
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        string PlantId,
        double? LeafCount,
        double? width,
        double? height,
        double? area,
        string? Disease,
        string? Health,
        DateTime? AnalysisDate,
        string? ImageName
    );
}
