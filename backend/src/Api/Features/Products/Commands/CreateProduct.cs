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

namespace Fei.Is.Api.Features.Products.Commands;

public static class CreateProduct
{
    public record Request(
        string PLUCode,
        string Code,
        string LatinName,
        string? CzechName,
        string? FlowerLeafDescription,
        string? PotDiameterPack,
        decimal? pricePerPiecePack,
        decimal? pricePerPiecePackVAT,
        decimal? discountedPriceWithoutVAT,
        decimal? RetailPrice,
        Guid CategoryId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "products",
                    async Task<Results<Created<String>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
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

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<String>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<String>>
    {
        public async Task<Result<String>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var category = await context.Categories.FindAsync([message.Request.CategoryId, cancellationToken], cancellationToken: cancellationToken);
            if (category is null)
            {
                return Result.Fail(new NotFoundError());
            }

            var product = new Product
            {
                PLUCode = message.Request.PLUCode,
                Code = message.Request.Code,
                LatinName = message.Request.LatinName,
                CzechName = message.Request.CzechName,
                FlowerLeafDescription = message.Request.FlowerLeafDescription,
                PotDiameterPack = message.Request.PotDiameterPack,
                PricePerPiecePack = message.Request.pricePerPiecePack,
                PricePerPiecePackVAT = message.Request.pricePerPiecePackVAT,
                DiscountedPriceWithoutVAT = message.Request.discountedPriceWithoutVAT,
                RetailPrice = message.Request.RetailPrice,
                Category = category
            };

            await context.Products.AddAsync(product, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(product.PLUCode);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.PLUCode).NotEmpty().WithMessage("PLUCode is required");
            RuleFor(r => r.Request.LatinName).NotEmpty().WithMessage("LatinName is required");
            RuleFor(r => r.Request.CategoryId).NotEmpty().WithMessage("CategoryId is required");
        }
    }
}
