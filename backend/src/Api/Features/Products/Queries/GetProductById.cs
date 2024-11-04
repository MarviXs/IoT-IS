using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Queries;

public static class GetProductById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "products/{id:Guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, Guid id) =>
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
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a product by PLUCode";
                    return o;
                });
        }
    }

    public record Query(Guid Id) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await context.Products.AsNoTracking()
                .Include(p => p.Category)
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
                PricePerPiecePackVAT: product.PricePerPiecePackVAT,
                DiscountedPriceWithoutVAT: product.DiscountedPriceWithoutVAT,
                RetailPrice: product.RetailPrice,
                CategoryId: product.Category.Id
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        string PLUCode,
        string Code,
        string LatinName,
        string? CzechName,
        string? FlowerLeafDescription,
        string? PotDiameterPack,
        decimal? PricePerPiecePack,
        decimal? PricePerPiecePackVAT,
        decimal? DiscountedPriceWithoutVAT,
        decimal? RetailPrice,
        Guid CategoryId
    );
}
