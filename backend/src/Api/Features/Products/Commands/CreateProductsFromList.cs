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

public static class CreateProductsFromList
{
    public record Request(List<ProductRequest> Products, Guid CategoryId);

    public record ProductRequest(
        string PLUCode,
        string Code,
        string LatinName,
        string? CzechName,
        string? FlowerLeafDescription,
        string? PotDiameterPack,
        decimal? pricePerPiecePack,
        decimal? pricePerPiecePackVAT,
        decimal? discountedPriceWithoutVAT,
        decimal? RetailPrice
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

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command message, CancellationToken cancellationToken)
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

            await context.Products.AddRangeAsync(
                message.Request.Products.Select(
                    (item) =>
                    {
                        return new Product
                        {
                            PLUCode = item.PLUCode,
                            Code = item.Code,
                            LatinName = item.LatinName,
                            CzechName = item.CzechName,
                            FlowerLeafDescription = item.FlowerLeafDescription,
                            PotDiameterPack = item.PotDiameterPack,
                            PricePerPiecePack = item.pricePerPiecePack,
                            PricePerPiecePackVAT = item.pricePerPiecePackVAT,
                            DiscountedPriceWithoutVAT = item.discountedPriceWithoutVAT,
                            RetailPrice = item.RetailPrice,
                            Category = category
                        };
                    }
                ),
                cancellationToken
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
            RuleFor(r => r.Request.CategoryId).NotEmpty().WithMessage("CategoryId is required");
        }
    }
}
