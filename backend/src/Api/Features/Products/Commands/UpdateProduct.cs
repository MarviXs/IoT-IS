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
    public static class UpdateProduct
    {
        // Rozšírený Request record s novými vlastnosťami pre Product
        public record Request(
            string PLUCode,
            string? EANCode,
            string? Code,
            string LatinName,
            string? CzechName,
            string? FlowerLeafDescription,
            string Variety,
            string? PotDiameterPack,
            decimal? PricePerPiecePack,
            decimal? pricePerPiecePackVAT,
            decimal? DiscountedPriceWithoutVAT,
            decimal? RetailPrice,
            Guid CategoryId,
            Guid? SupplierId,
            Guid? VATCategoryId,
            string? CCode,
            string? Country,
            string? City,
            int? GreenhouseNumber,
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
                app.MapPut(
                        "products/{id:Guid}",
                        async Task<Results<NotFound, ValidationProblem, NoContent>> (
                            IMediator mediator,
                            Guid id,
                            Request request) =>
                        {
                            var command = new Command(request, id);
                            var result = await mediator.Send(command);
                            if (result.HasError<NotFoundError>())
                            {
                                return TypedResults.NotFound();
                            }
                            else if (result.HasError<ValidationError>())
                            {
                                return TypedResults.ValidationProblem(result.ToValidationErrors());
                            }
                            return TypedResults.NoContent();
                        }
                    )
                    .WithName(nameof(UpdateProduct))
                    .WithTags(nameof(Product))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Update a product";
                        return o;
                    });
            }
        }

        public record Command(Request Request, Guid Id) : IRequest<Result>;

        public sealed class Handler(AppDbContext context, IValidator<Command> validator)
            : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
            {
                var validationResult = await validator.ValidateAsync(message, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return Result.Fail(new ValidationError(validationResult));
                }

                var product = await context.Products
                                           .Include(p => p.Category)
                                           .Include(p => p.Supplier)
                                           .Include(p => p.VATCategory)
                                           .FirstOrDefaultAsync(p => p.Id == message.Id, cancellationToken);
                if (product == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Ak sa zmenil PLUCode, overíme, či už neexistuje produkt s rovnakým PLUCode
                if (product.PLUCode != message.Request.PLUCode)
                {
                    if (await context.Products.AnyAsync(p => p.PLUCode == message.Request.PLUCode, cancellationToken))
                    {
                        return Result.Fail(new BadRequestError("Product with this PLU code already exists"));
                    }
                }

                // Aktualizácia základných vlastností
                product.PLUCode = message.Request.PLUCode;
                product.Code = message.Request.Code;
                product.LatinName = message.Request.LatinName;
                product.CzechName = message.Request.CzechName;
                product.FlowerLeafDescription = message.Request.FlowerLeafDescription;
                product.Variety = message.Request.Variety;
                product.PotDiameterPack = message.Request.PotDiameterPack;
                product.PricePerPiecePack = message.Request.PricePerPiecePack;
                product.DiscountedPriceWithoutVAT = message.Request.DiscountedPriceWithoutVAT;
                product.RetailPrice = message.Request.RetailPrice;
                product.CCode = message.Request.CCode;
                product.Country = message.Request.Country;
                product.City = message.Request.City;
                product.GreenhouseNumber = message.Request.GreenhouseNumber;
            

                // Aktualizácia kategórie, ak sa zmenila
                if (message.Request.CategoryId != product.Category.Id)
                {
                    var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == message.Request.CategoryId, cancellationToken);
                    if (category == null)
                    {
                        return Result.Fail(new NotFoundError());
                    }
                    product.Category = category;
                }

                // Aktualizácia dodávateľa, ak bol poskytnutý a zmenil sa
                if (message.Request.SupplierId != null && message.Request.SupplierId != product.Supplier?.Id)
                {
                    var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.Id == message.Request.SupplierId, cancellationToken);
                    if (supplier == null)
                    {
                        return Result.Fail(new NotFoundError());
                    }
                    product.Supplier = supplier;
                }

                // Aktualizácia VAT kategórie, ak bol poskytnutý a zmenil sa
                if (message.Request.VATCategoryId != null && message.Request.VATCategoryId != product.VATCategory?.Id)
                {
                    var vatCategory = await context.VATCategories.FirstOrDefaultAsync(v => v.Id == message.Request.VATCategoryId, cancellationToken);
                    if (vatCategory == null)
                    {
                        return Result.Fail(new NotFoundError());
                    }
                    product.VATCategory = vatCategory;
                }

                // Aktualizácia nových vlastností
                product.HeightCm = message.Request.HeightCm;
                product.SeedsPerThousandPlants = message.Request.SeedsPerThousandPlants;
                product.SeedsPerThousandPots = message.Request.SeedsPerThousandPots;
                product.SowingPeriod = message.Request.SowingPeriod;
                product.GerminationTemperatureC = message.Request.GerminationTemperatureC;
                product.GerminationTimeDays = message.Request.GerminationTimeDays;
                product.CultivationTimeSowingToPlant = message.Request.CultivationTimeSowingToPlant;
                product.SeedsMioHa = message.Request.SeedsMioHa;
                product.SeedSpacingCM = message.Request.SeedSpacingCM;
                product.CultivationTimeVegetableWeek = message.Request.CultivationTimeVegetableWeek;
                product.BulbPlantingRequirementSqM = message.Request.BulbPlantingRequirementSqM;
                product.BulbPlantingPeriod = message.Request.BulbPlantingPeriod;
                product.BulbPlantingDistanceCm = message.Request.BulbPlantingDistanceCm;
                product.CultivationTimeForBulbsWeeks = message.Request.CultivationTimeForBulbsWeeks;
                product.NumberOfBulbsPerPot = message.Request.NumberOfBulbsPerPot;
                product.PlantSpacingCm = message.Request.PlantSpacingCm;
                product.PotSizeCm = message.Request.PotSizeCm;
                product.CultivationTimeFromYoungPlant = message.Request.CultivationTimeFromYoungPlant;
                product.CultivationTemperatureC = message.Request.CultivationTemperatureC;
                product.NaturalFloweringMonth = message.Request.NaturalFloweringMonth;
                product.FlowersInFirstYear = message.Request.FlowersInFirstYear;
                product.GrowthInhibitorsUsed = message.Request.GrowthInhibitorsUsed;
                product.PlantingDensity = message.Request.PlantingDensity;

                await context.SaveChangesAsync(cancellationToken);

                return Result.Ok();
            }
        }

        public record Response(Guid Id, string Name, long? ResponseTime, long? LastResponseTimestamp);

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(r => r.Request.LatinName).NotEmpty().WithMessage("LatinName is required");
                RuleFor(r => r.Request.CategoryId).NotEmpty().WithMessage("CategoryId is required");
                RuleFor(r => r.Request.PLUCode).NotEmpty().WithMessage("PLUCode is required");
                // Pridajte ďalšie validácie pre nové polia podľa potreby
            }
        }
    }
}
