using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Queries
{
    public static class GetProductById
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                    "products/{id:Guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>>(
                        IMediator mediator,
                        Guid id) =>
                    {
                        var query = new Query(id);
                        var result = await mediator.Send(query);
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetProductById))
                .WithTags(nameof(Product))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a product by id";
                    return o;
                });
            }
        }

        public record Query(Guid Id) : IRequest<Result<Response>>;

        public sealed class Handler : IRequestHandler<Query, Result<Response>>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Include(p => p.VATCategory)
                    .FirstOrDefaultAsync(product => product.Id == request.Id, cancellationToken);

                if (product == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                var response = new Response(
                    PLUCode: product.PLUCode,
                    Code: product.Code,
                    LatinName: product.LatinName,
                    CzechName: product.CzechName,
                    FlowerLeafDescription: product.FlowerLeafDescription,
                    PotDiameterPack: product.PotDiameterPack,
                    PricePerPiecePack: product.PricePerPiecePack,
                    DiscountedPriceWithoutVAT: product.DiscountedPriceWithoutVAT,
                    RetailPrice: product.RetailPrice,
                    Category: new CategoryModel(product.Category.Id, product.Category.CategoryName),
                    Supplier: new SupplierModel(product.Supplier.Id, product.Supplier.Name),
                    Variety: product.Variety,
                    VATCategory: new VatCategoryModel(product.VATCategory.Id, product.VATCategory.Name, product.VATCategory.Rate),
                    // Nové vlastnosti:
                    HeightCm: product.HeightCm,
                    SeedsPerThousandPlants: product.SeedsPerThousandPlants,
                    SeedsPerThousandPots: product.SeedsPerThousandPots,
                    SowingPeriod: product.SowingPeriod,
                    GerminationTemperatureC: product.GerminationTemperatureC,
                    GerminationTimeDays: product.GerminationTimeDays,
                    CultivationTimeSowingToPlant: product.CultivationTimeSowingToPlant,
                    SeedsMioHa: product.SeedsMioHa,
                    SeedSpacingCM: product.SeedSpacingCM,
                    CultivationTimeVegetableWeek: product.CultivationTimeVegetableWeek,
                    BulbPlantingRequirementSqM: product.BulbPlantingRequirementSqM,
                    BulbPlantingPeriod: product.BulbPlantingPeriod,
                    BulbPlantingDistanceCm: product.BulbPlantingDistanceCm,
                    CultivationTimeForBulbsWeeks: product.CultivationTimeForBulbsWeeks,
                    NumberOfBulbsPerPot: product.NumberOfBulbsPerPot,
                    PlantSpacingCm: product.PlantSpacingCm,
                    PotSizeCm: product.PotSizeCm,
                    CultivationTimeFromYoungPlant: product.CultivationTimeFromYoungPlant,
                    CultivationTemperatureC: product.CultivationTemperatureC,
                    NaturalFloweringMonth: product.NaturalFloweringMonth,
                    FlowersInFirstYear: product.FlowersInFirstYear,
                    GrowthInhibitorsUsed: product.GrowthInhibitorsUsed,
                    PlantingDensity: product.PlantingDensity
                );

                return Result.Ok(response);
            }
        }

        public record Response(
            string PLUCode,
            string? Code,
            string LatinName,
            string? CzechName,
            string? FlowerLeafDescription,
            string? PotDiameterPack,
            decimal? PricePerPiecePack,
            decimal? DiscountedPriceWithoutVAT,
            decimal? RetailPrice,
            CategoryModel Category,
            SupplierModel Supplier,
            string Variety,
            VatCategoryModel VATCategory,
            // Nové vlastnosti:
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

        public record CategoryModel(
            Guid Id,
            string Name
        );

        public record SupplierModel(
            Guid Id,
            string Name
        );

        public record VatCategoryModel(
            Guid Id,
            string Name,
            decimal Rate
        );
    }
}
