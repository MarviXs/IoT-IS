using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Products.Queries;

public static class GetProducts
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "products",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetProducts))
                .WithTags(nameof(Product))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated products";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            List<Product> products = new List<Product>()
            {
                new Product()
                {
                    PLUCode = 1564,
                    Code = "Code",
                    LatinName = "LatinName",
                    CzechName = "Agaty",
                    FlowerLeafDescription = "FlowerLeafDescription",
                    PotDiameterPack = "PotDiameterPack",
                    PricePerPiecePack = 1,
                    PricePerPiecePackVAT = 1,
                    DiscountedPriceWithoutVAT = 1,
                    RetailPrice = 1,
                    CategoryId = 1,
                    Category = new Category()
                },
                new Product()
                {
                    PLUCode = 2238,
                    Code = "Code",
                    LatinName = "LatinName",
                    CzechName = "Ruze",
                    FlowerLeafDescription = "FlowerLeafDescription",
                    PotDiameterPack = "PotDiameterPack",
                    PricePerPiecePack = 1,
                    PricePerPiecePackVAT = 1,
                    DiscountedPriceWithoutVAT = 1,
                    RetailPrice = 1,
                    CategoryId = 1,
                    Category = new Category()
                },
                new Product()
                {
                    PLUCode = 3491,
                    Code = "Code",
                    LatinName = "LatinName",
                    CzechName = "Sedmokrasky",
                    FlowerLeafDescription = "FlowerLeafDescription",
                    PotDiameterPack = "PotDiameterPack",
                    PricePerPiecePack = 1,
                    PricePerPiecePackVAT = 1,
                    DiscountedPriceWithoutVAT = 1,
                    RetailPrice = 1,
                    CategoryId = 1,
                    Category = new Category()
                }
            };

            return products.Select((product) => new Response(product.PLUCode, product.CzechName, product.RetailPrice)).ToPagedList(products.Count, 0, 10);
        }
    }

    public record Response(int pluCode, string czechName, decimal retailPrice);
}
