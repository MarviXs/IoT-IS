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

namespace Fei.Is.Api.Features.Products.Commands;

public static class CreateProduct
{
    public record Request(
        string Code,
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
        EVatCategory VATCategory
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

            var supplier = context.Suppliers.Where(supplier => supplier.Id == message.Request.SupplierId);
            if (!await supplier.AnyAsync(cancellationToken))
            {
                return Result.Fail(new BadRequestError("Supplier does not exists"));
            }

            var product = new Product
            {
                PLUCode = await pluCodeService.GetPLUCodeAsync(cancellationToken),
                Code = message.Request.Code,
                LatinName = message.Request.LatinName,
                CzechName = message.Request.CzechName,
                FlowerLeafDescription = message.Request.FlowerLeafDescription,
                PotDiameterPack = message.Request.PotDiameterPack,
                PricePerPiecePack = message.Request.pricePerPiecePack,
                DiscountedPriceWithoutVAT = message.Request.discountedPriceWithoutVAT,
                RetailPrice = message.Request.RetailPrice,
                Supplier = await supplier.FirstAsync(cancellationToken),
                Variety = message.Request.Variety,
                VATCategory = message.Request.VATCategory
            };

            var category = context.Categories.Where(category => category.CategoryName == message.Request.CategoryName);
            if (!await category.AnyAsync(cancellationToken))
            {
                return Result.Fail(new BadRequestError("Category does not exists"));
            }

            product.Category = await category.FirstAsync(cancellationToken);

            await context.Products.AddAsync(product, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(product.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.CategoryName).NotEmpty().WithMessage("CategoryName is required");
            RuleFor(r => r.Request.SupplierId).NotEmpty().WithMessage("SupplierId is required");
            RuleFor(r => r.Request.Variety).NotEmpty().WithMessage("Variety is required");
            RuleFor(r => r.Request.VATCategory).NotEmpty().WithMessage("VAT Category is required");
        }
    }
}
