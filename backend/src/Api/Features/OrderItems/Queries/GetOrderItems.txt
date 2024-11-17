using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.OrderItems.Queries;

public static class GetOrderItems
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "orders/{orderId}/items",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, int orderId, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, orderId, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetOrderItems))
                .WithTags(nameof(OrderItem))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated order items by order ID";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, int OrderId, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == message.OrderId) // Filter podľa OrderId
                .Include(oi => oi.Product) // Načítanie priradenej produktovej entity, ak je potrebná
                .Select(orderItem => new Response(
                    orderItem.Id,
                    orderItem.OrderId,
                    orderItem.ProductNumber,
                    orderItem.VarietyName,
                    orderItem.Quantity
                ))
                .Paginate(message.Parameters);  // Použitie stránkovania

            return query.ToPagedList(await query.CountAsync(cancellationToken), message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(int Id, int OrderId, Guid ProductNumber, string VarietyName, int Quantity);
}
