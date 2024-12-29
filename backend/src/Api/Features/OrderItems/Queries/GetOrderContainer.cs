using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
                    async Task<Results<NotFound, Ok<PagedList<Response>>>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid orderId,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, orderId, parameters);
                        var result = await mediator.Send(query);
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
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

    public record Query(ClaimsPrincipal User, Guid OrderId, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var orderQuery = context.Orders.Include(o => o.ItemContainers).ThenInclude(ic => ic.Items).Where(o => o.Id == message.OrderId);

            if (!await orderQuery.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            var order = await orderQuery.FirstAsync(cancellationToken);

            var itemContainters = order.ItemContainers.Select(container => new Response(
                container.Id,
                container.Name,
                container.Quantity,
                container.PricePerContainer,
                container.TotalPrice,
                context.OrderItems.Select(oi => new ProductResponse(
                    oi.Product.PLUCode,
                    oi.Product.LatinName,
                    oi.Product.CzechName,
                    oi.Product.Variety,
                    oi.Product.PotDiameterPack,
                    oi.Product.PricePerPiecePack,
                    oi.Product.PricePerPiecePack, //TODO doratavat vat
                    oi.Quantity
                ))
            ));

            // Vytvorenie stránkovania
            return itemContainters.ToPagedList(itemContainters.Count(), message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(Guid Id, string Name, int Quantity, decimal? PricePerContainer, decimal? TotalPrice, IQueryable<ProductResponse> Products);

    public record ProductResponse(
        string PLUCode,
        string LatinName,
        string? CzechName,
        string Variety,
        string? PotDiameterPack,
        decimal? PricePerPiecePack,
        decimal? PricePerPiecePackVAT,
        int Quantity
    );
}
