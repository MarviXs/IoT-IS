using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Orders.Queries
{
    public static class GetOrderProducts
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                        "orders/{id}/products",
                        async Task<Results<NotFound, Ok<List<Response>>>> (
                            IMediator mediator,
                            ClaimsPrincipal user,
                            Guid id) =>
                        {
                            var query = new Query(user, id);
                            var result = await mediator.Send(query);

                            if (result.IsFailed && result.HasError<NotFoundError>())
                                return TypedResults.NotFound();

                            return TypedResults.Ok(result.Value!);
                        }
                    )
                    .WithName(nameof(GetOrderProducts))
                    .WithTags(nameof(Order))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Get all products of an order with LatinName, CzechName, etc.";
                        return o;
                    });
            }
        }

        public record Query(ClaimsPrincipal User, Guid OrderId) : IRequest<Result<List<Response>>>;

        public sealed class Handler : IRequestHandler<Query, Result<List<Response>>>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context) => _context = context;

            public async Task<Result<List<Response>>> Handle(Query message, CancellationToken ct)
            {
                var products = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.Id == message.OrderId)
                    .SelectMany(o => o.ItemContainers)
                    .SelectMany(ic => ic.Items)
                    .Select(oi => new Response(
                        oi.Product.Id,
                        oi.Product.LatinName,
                        oi.Product.CzechName,
                        oi.Product.FlowerLeafDescription,
                        oi.Product.PotDiameterPack,
                        oi.Product.GreenhouseNumber,
                        oi.Product.HeightCm,
                        oi.Product.SeedsPerThousandPlants,
                        oi.Product.SeedsPerThousandPots,
                        oi.Product.SowingPeriod,
                        oi.Product.GerminationTemperatureC,
                        oi.Product.GerminationTimeDays,
                        oi.Product.CultivationTimeSowingToPlant,
                        oi.Product.SeedsMioHa,
                        oi.Product.SeedSpacingCM,
                        oi.Product.CultivationTimeVegetableWeek,
                        oi.Product.BulbPlantingRequirementSqM,
                        oi.Product.BulbPlantingPeriod,
                        oi.Product.BulbPlantingDistanceCm,
                        oi.Product.CultivationTimeForBulbsWeeks,
                        oi.Product.NumberOfBulbsPerPot,
                        oi.Product.PlantSpacingCm,
                        oi.Product.PotSizeCm,
                        oi.Product.CultivationTimeFromYoungPlant,
                        oi.Product.CultivationTemperatureC,
                        oi.Product.NaturalFloweringMonth,
                        oi.Product.FlowersInFirstYear,
                        oi.Product.GrowthInhibitorsUsed,
                        oi.Product.PlantingDensity
                    ))
                    .ToListAsync(ct);

                if (products.Count == 0)
                    return Result.Fail(new NotFoundError());

                return Result.Ok(products);
            }
        }

        public record Response(
            Guid ProductId,
            string LatinName,
            string? CzechName,
            string? FlowerLeafDescription,
            string? PotDiameterPack,
            int? GreenhouseNumber,
            string? HeightCm,
            string? SeedsPerThousandPlants,
            string? SeedsPerThousandPots,
            string? SowingPeriod,
            string? GerminationTemperatureC,
            string? GerminationTimeDays,
            string? CultivationTimeSowingToPlant,
            string? SeedsMioHa,
            string? SeedSpacingCM,
            string? CultivationTimeVegetableWeek,
            string? BulbPlantingRequirementSqM,
            string? BulbPlantingPeriod,
            string? BulbPlantingDistanceCm,
            string? CultivationTimeForBulbsWeeks,
            string? NumberOfBulbsPerPot,
            string? PlantSpacingCm,
            string? PotSizeCm,
            string? CultivationTimeFromYoungPlant,
            string? CultivationTemperatureC,
            string? NaturalFloweringMonth,
            bool? FlowersInFirstYear,
            bool? GrowthInhibitorsUsed,
            string? PlantingDensity
        );
    }
}
