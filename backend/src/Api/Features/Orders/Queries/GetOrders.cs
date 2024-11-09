using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

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

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            List<Order> orders = new List<Order>()
            {
                new Order()
                {
                    Id = 1,
                    Note = "First Order",
                    CustomerId = 1,
                    OrderDate = DateTime.UtcNow.AddDays(-10),
                    DeliveryWeek = 45,
                    PaymentMethod = "Credit Card",
                    ContactPhone = "+123456789",
                    Customer = new Company { Id = 1, Title = "Customer One" },
                },
                new Order()
                {
                    Id = 2,
                    Note = "Second Order",
                    CustomerId = 2,
                    OrderDate = DateTime.UtcNow.AddDays(-7),
                    DeliveryWeek = 46,
                    PaymentMethod = "Bank Transfer",
                    ContactPhone = "+987654321",
                    Customer = new Company { Id = 2, Title = "Customer Two" },
                },
                new Order()
                {
                    Id = 3,
                    Note = "Third Order",
                    CustomerId = 3,
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    DeliveryWeek = 47,
                    PaymentMethod = "Cash on Delivery",
                    ContactPhone = "+192837465",
                    Customer = new Company { Id = 3, Title = "Customer Three" },
                }
            };

            return orders
                .Select(order => new Response(
                    order.Id,
                    order.Customer.Title,
                    order.OrderDate,
                    order.DeliveryWeek,
                    order.PaymentMethod,
                    order.ContactPhone,
                    order.Note
                ))
                .ToPagedList(orders.Count, 0, 10);
        }
    }

    public record Response(int id, string customerName, DateTime orderDate, int deliveryWeek, string paymentMethod, string contactPhone, string note);
}
