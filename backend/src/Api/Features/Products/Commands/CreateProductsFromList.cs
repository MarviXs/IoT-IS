using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Services.PLUCode;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Commands;

public static class CreateProductsFromList
{
    public record Request(List<ProductRequest> Products);

    public static class EanGenerator
    {
        private const string Prefix = "859"; // GS1 prefix pro ČR/SR

        public static string GenerateEan13FromPlu(string pluCode)
        {
            var paddedPlu = pluCode.PadLeft(9, '0');
            var baseCode = Prefix + paddedPlu;

            int sum = 0;
            for (int i = 0; i < baseCode.Length; i++)
            {
                int digit = baseCode[i] - '0';
                sum += digit * ((i % 2 == 0) ? 1 : 3);
            }

            var mod = sum % 10;
            var checkDigit = mod == 0 ? 0 : 10 - mod;

            return baseCode + checkDigit;
        }
    }

    public record ProductRequest(
        string? Code,
        string LatinName,
        string? CzechName,
        string? FlowerLeafDescription,
        string? PotDiameterPack,
        decimal? pricePerPiecePack,
        decimal? pricePerPiecePackVAT,
        decimal? discountedPriceWithoutVAT,
        decimal? RetailPrice,
        string CategoryName,
        Guid SupplierId,
        string Variety,
        Guid VATCategoryId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "products-by-list",
                    async Task<Results<Created<int>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
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
                .WithName(nameof(CreateProductsFromList))
                .WithTags(nameof(Product))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create products from list";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<int>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator, PLUCodeService pluCodeService)
        : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var currentSuppliers = await context.Suppliers.AsNoTracking().ToListAsync(cancellationToken);

            if (message.Request.Products.Any(product => currentSuppliers.Any(supplier => product.SupplierId != supplier.Id)))
            {
                return Result.Fail(new BadRequestError());
            }

            await context.Products.AddRangeAsync(
                message.Request.Products.Select(item =>
                {
                    var pluCode = pluCodeService.GetPLUCode();
                    var eanCode = EanGenerator.GenerateEan13FromPlu(pluCode);
                    var product = new Product()
                    {
                        PLUCode = pluCode,
                        Code = item.Code,
                        LatinName = item.LatinName,
                        CzechName = item.CzechName,
                        FlowerLeafDescription = item.FlowerLeafDescription,
                        PotDiameterPack = item.PotDiameterPack,
                        PricePerPiecePack = item.pricePerPiecePack,
                        DiscountedPriceWithoutVAT = item.discountedPriceWithoutVAT,
                        RetailPrice = item.RetailPrice,
                        Variety = item.Variety,
                        Supplier = currentSuppliers.First(supplier => supplier.Id == item.SupplierId)
                    };

                    var category = context.Categories.Where(category => category.CategoryName == item.CategoryName);
                    if (!category.Any())
                    {
                        var entityEntry = context.Categories.Add(
                            new Category
                            {
                                Id = Guid.NewGuid(),
                                CategoryName = item.CategoryName,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            }
                        );

                        product.Category = entityEntry.Entity;
                    }
                    else
                    {
                        product.Category = category.First();
                    }

                    var vatCategory = context.VATCategories.Where(vatCategory => vatCategory.Id == item.VATCategoryId);
                    if (vatCategory.Any())
                    {
                        product.VATCategory = vatCategory.First();
                    }
                    else
                    {
                        product.VATCategory = context.VATCategories.First();
                    }

                    return product;
                })
            );

            int addedRows = await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(addedRows);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.Products).NotEmpty().WithMessage("Products can't be empty");
            RuleForEach(r => r.Request.Products).SetValidator(new ProductValidator());
        }
    }

    public sealed class ProductValidator : AbstractValidator<ProductRequest>
    {
        public ProductValidator()
        {
            RuleFor(r => r.LatinName).NotEmpty().WithMessage("LatinName is required");
            RuleFor(r => r.CategoryName).NotEmpty().WithMessage("CategoryName is required");
            RuleFor(r => r.SupplierId).NotEmpty().WithMessage("SupplierId is required");
            RuleFor(r => r.Variety).NotEmpty().WithMessage("Variety is required");
            RuleFor(r => r.VATCategoryId).NotEmpty().WithMessage("VAT Category is required");
        }
    }
}
