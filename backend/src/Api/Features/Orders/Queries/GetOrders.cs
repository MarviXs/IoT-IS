using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Orders.Queries;

public static class GetOrders
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "orders",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetOrders))
                .WithTags(nameof(Order))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated orders";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context
                .Orders.AsNoTracking()
                .Include(o => o.Customer) // Načítanie priradenej zákazníckej entity
                .Select(order => new Response(
                    order.Id,
                    order.Customer.Title, // Prístup k názvu zákazníka cez navigačnú vlastnosť
                    order.OrderDate,
                    order.DeliveryWeek,
                    order.PaymentMethod,
                    order.ContactPhone,
                    order.Note
                ))
                .Paginate(message.Parameters); // Použitie stránkovania

            return query.ToPagedList(await query.CountAsync(cancellationToken), message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(
        Guid Id,
        string CustomerName,
        DateTime OrderDate,
        int DeliveryWeek,
        string PaymentMethod,
        string ContactPhone,
        string? Note
    );
}
