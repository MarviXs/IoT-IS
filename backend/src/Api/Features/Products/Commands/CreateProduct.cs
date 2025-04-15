using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Commands
{
    public static class CreateProduct
    {
        // Rozšírený Request record vrátane nových polí pre Product
        public record Request(
            string? Code,
            string? PLUCode,
            string LatinName,
            string? CzechName,
            string? FlowerLeafDescription,
            string? PotDiameterPack,
            decimal? PricePerPiecePack,
            decimal? pricePerPiecePackVAT,
            decimal? DiscountedPriceWithoutVAT,
            decimal? RetailPrice,
            Guid CategoryId,
            Guid SupplierId,
            string Variety,
            Guid VATCategoryId,
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

        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost(
                        "products",
                        async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                        {
                            var command = new Command(request, user);

                            var result = await mediator.Send(command);

                            if (result.HasError<ValidationError>())
                            {
                                return TypedResults.ValidationProblem(result.ToValidationErrors());
                            }

                            return TypedResults.Created(result.Value.ToString(), result.Value);
                        }
                    )
                    .WithName(nameof(CreateProduct))
                    .WithTags(nameof(Product))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Create a product";
                        return o;
                    });
            }
        }

        public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

        public sealed class Handler(AppDbContext context, IValidator<Command> validator, PLUCodeService pluCodeService)
            : IRequestHandler<Command, Result<Guid>>
        {
            public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
            {
                var result = await validator.ValidateAsync(message, cancellationToken);
                if (!result.IsValid)
                {
                    return Result.Fail(new ValidationError(result));
                }

                if (context.Products.Any(p => p.Code == message.Request.Code))
                {
                    return Result.Fail(new BadRequestError("Product with this code already exists"));
                }

                var supplier = context.Suppliers.Where(s => s.Id == message.Request.SupplierId);
                if (!await supplier.AnyAsync(cancellationToken))
                {
                    return Result.Fail(new BadRequestError("Supplier does not exist"));
                }

                var vatCategory = context.VATCategories.Where(v => v.Id == message.Request.VATCategoryId);
                if (!await vatCategory.AnyAsync(cancellationToken))
                {
                    return Result.Fail(new BadRequestError("VAT Category does not exist"));
                }

                var pluCode = string.IsNullOrEmpty(message.Request.PLUCode)
                    ? await pluCodeService.GetPLUCodeAsync(cancellationToken)
                    : message.Request.PLUCode;

                var category = context.Categories.Where(c => c.Id == message.Request.CategoryId);
                if (!await category.AnyAsync(cancellationToken))
                {
                    return Result.Fail(new BadRequestError("Category does not exist"));
                }

                var product = new Product
                {
                    PLUCode = pluCode,
                    Code = message.Request.Code,
                    LatinName = message.Request.LatinName,
                    CzechName = message.Request.CzechName,
                    FlowerLeafDescription = message.Request.FlowerLeafDescription,
                    PotDiameterPack = message.Request.PotDiameterPack,
                    PricePerPiecePack = message.Request.PricePerPiecePack,
                    DiscountedPriceWithoutVAT = message.Request.DiscountedPriceWithoutVAT,
                    RetailPrice = message.Request.RetailPrice,
                    Supplier = await supplier.FirstAsync(cancellationToken),
                    Variety = message.Request.Variety,
                    VATCategory = await vatCategory.FirstAsync(cancellationToken),
                    Category = await category.FirstAsync(cancellationToken),
                    // Nové vlastnosti:
                    HeightCm = message.Request.HeightCm,
                    SeedsPerThousandPlants = message.Request.SeedsPerThousandPlants,
                    SeedsPerThousandPots = message.Request.SeedsPerThousandPots,
                    SowingPeriod = message.Request.SowingPeriod,
                    GerminationTemperatureC = message.Request.GerminationTemperatureC,
                    GerminationTimeDays = message.Request.GerminationTimeDays,
                    CultivationTimeSowingToPlant = message.Request.CultivationTimeSowingToPlant,
                    SeedsMioHa = message.Request.SeedsMioHa,
                    SeedSpacingCM = message.Request.SeedSpacingCM,
                    CultivationTimeVegetableWeek = message.Request.CultivationTimeVegetableWeek,
                    BulbPlantingRequirementSqM = message.Request.BulbPlantingRequirementSqM,
                    BulbPlantingPeriod = message.Request.BulbPlantingPeriod,
                    BulbPlantingDistanceCm = message.Request.BulbPlantingDistanceCm,
                    CultivationTimeForBulbsWeeks = message.Request.CultivationTimeForBulbsWeeks,
                    NumberOfBulbsPerPot = message.Request.NumberOfBulbsPerPot,
                    PlantSpacingCm = message.Request.PlantSpacingCm,
                    PotSizeCm = message.Request.PotSizeCm,
                    CultivationTimeFromYoungPlant = message.Request.CultivationTimeFromYoungPlant,
                    CultivationTemperatureC = message.Request.CultivationTemperatureC,
                    NaturalFloweringMonth = message.Request.NaturalFloweringMonth,
                    FlowersInFirstYear = message.Request.FlowersInFirstYear,
                    GrowthInhibitorsUsed = message.Request.GrowthInhibitorsUsed,
                    PlantingDensity = message.Request.PlantingDensity
                };

                await context.Products.AddAsync(product, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return Result.Ok(product.Id);
            }
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(r => r.Request.CategoryId).NotEmpty().WithMessage("CategoryId is required");
                RuleFor(r => r.Request.SupplierId).NotEmpty().WithMessage("SupplierId is required");
                RuleFor(r => r.Request.Variety).NotEmpty().WithMessage("Variety is required");
                RuleFor(r => r.Request.VATCategoryId).NotEmpty().WithMessage("VAT Category Id is required");
                // Pridajte validácie pre nové polia podľa potreby
            }
        }
    }
}
