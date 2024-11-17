using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.OrderItemContainers.Queries;

public static class GetOrderItemContainer
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "orders/{orderId}/container",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, int orderId, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, orderId, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetOrderItemContainer))
                .WithTags(nameof(OrderItemContainer))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated order item containers by order ID";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, int OrderId, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            // Načítanie kontajnerov s prepojenými produktmi
            var query = context.OrderItemContainers
                .AsNoTracking()
                .Where(oic => oic.OrderId == message.OrderId) // Filtrovanie podľa objednávky
                .Select(container => new Response(
                    container.Id,
                    container.OrderId,
                    container.Name,
                    container.Quantity,
                    container.PricePerContainer ?? 0m,
                    container.TotalPrice ?? 0m,
                    context.OrderItems // Načítanie produktov pre tento kontajner
                        .Where(oi => oi.ContainerId == container.Id)
                        .Select(oi => new ProductResponse(
                            oi.Product.PLUCode,
                            oi.Product.LatinName,
                            oi.Product.CzechName ?? string.Empty,
                            oi.Product.Variety ?? string.Empty,
                            oi.Product.PotDiameterPack ?? string.Empty,
                            oi.Product.PricePerPiecePack ?? 0m,
                            oi.Product.PricePerPiecePackVAT ?? 0m,
                            oi.Quantity
                        )).ToList()
                ));

            // Vytvorenie stránkovania
            return query.ToPagedList(
                await query.CountAsync(cancellationToken),
                message.Parameters.PageNumber,
                message.Parameters.PageSize
            );
        }
    }

    public record Response(
        int Id,
        int OrderId,
        string Name,
        int Quantity,
        decimal PricePerContainer,
        decimal TotalPrice,
        List<ProductResponse> Products
    );

    public record ProductResponse(
        string PLUCode,
        string LatinName,
        string CzechName,
        string Variety,
        string PotDiameterPack,
        decimal PricePerPiecePack,
        decimal PricePerPiecePackVAT,
        int Quantity
    );
}
