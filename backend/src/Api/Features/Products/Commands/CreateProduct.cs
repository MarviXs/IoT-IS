using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Services.EANCode;
using Fei.Is.Api.Services.PLUCode;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Commands
{
    public static class CreateProduct
    {
        public record Request(
            string? Code,
            string? PLUCode,
            string? EANCode,
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
            string? Country,
            string? City,
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
                                return TypedResults.ValidationProblem(result.ToValidationErrors());

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

        public sealed class Handler : IRequestHandler<Command, Result<Guid>>
        {
            private readonly AppDbContext _context;
            private readonly IValidator<Command> _validator;
            private readonly PLUCodeService _pluCodeService;
            private readonly EANCodeService _eanCodeService;

            public Handler(AppDbContext context, IValidator<Command> validator, PLUCodeService pluCodeService, EANCodeService eanCodeService)
            {
                _context = context;
                _validator = validator;
                _pluCodeService = pluCodeService;
                _eanCodeService = eanCodeService;
            }

            public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(message, cancellationToken);
                if (!validation.IsValid)
                    return Result.Fail(new ValidationError(validation));

                if (await _context.Products.AnyAsync(p => p.Code == message.Request.Code, cancellationToken))
                    return Result.Fail(new BadRequestError("Product with this code already exists"));

                if (!await _context.Suppliers.AnyAsync(s => s.Id == message.Request.SupplierId, cancellationToken))
                    return Result.Fail(new BadRequestError("Supplier does not exist"));

                if (!await _context.VATCategories.AnyAsync(v => v.Id == message.Request.VATCategoryId, cancellationToken))
                    return Result.Fail(new BadRequestError("VAT Category does not exist"));

                var pluCode = string.IsNullOrEmpty(message.Request.PLUCode)
                    ? await _pluCodeService.GetPLUCodeAsync(cancellationToken)
                    : message.Request.PLUCode!;

                if (!await _context.Categories.AnyAsync(c => c.Id == message.Request.CategoryId, cancellationToken))
                    return Result.Fail(new BadRequestError("Category does not exist"));

                var eanCode = _eanCodeService.FromPlu(pluCode);

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
                    Supplier = await _context.Suppliers.FirstAsync(s => s.Id == message.Request.SupplierId, cancellationToken),
                    Variety = message.Request.Variety,
                    VATCategory = await _context.VATCategories.FirstAsync(v => v.Id == message.Request.VATCategoryId, cancellationToken),
                    Category = await _context.Categories.FirstAsync(c => c.Id == message.Request.CategoryId, cancellationToken),
                    CCode = message.Request.Code,
                    Country = message.Request.Country,
                    City = message.Request.City,
                    GreenhouseNumber = message.Request.GreenhouseNumber,
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

                await _context.Products.AddAsync(product, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

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
            }
        }
    }
}
