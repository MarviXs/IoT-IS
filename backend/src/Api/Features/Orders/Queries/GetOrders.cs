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
                    Id = Guid.NewGuid(),
                    Note = "First Order",
                    OrderDate = DateTime.UtcNow.AddDays(-10),
                    DeliveryWeek = 45,
                    PaymentMethod = "Credit Card",
                    ContactPhone = "+123456789",
                    Customer = new Company
                    {
                        Id = Guid.Parse("3f1fa498-d863-473c-a8c0-b8c545df9cfd"),
                        Title = "Město Volyně",
                        Ic = "00252000",
                        Dic = "CZ00252000",
                        Street = "náměstí Svobody 41",
                        Psc = "387 01",
                        City = "Volyně"
                    },
                },
                new Order()
                {
                    Id = Guid.NewGuid(),
                    Note = "Second Order",
                    OrderDate = DateTime.UtcNow.AddDays(-7),
                    DeliveryWeek = 46,
                    PaymentMethod = "Bank Transfer",
                    ContactPhone = "+987654321",
                    Customer = new Company
                    {
                        Id = Guid.Parse("2a904cef-5184-4809-bdba-9a5a24171618"),
                        Title = "Technické služby města Volyně",
                        Ic = "00252000",
                        Dic = "CZ00252000",
                        Street = "náměstí Hrdinů 70",
                        Psc = "387 01",
                        City = "Volyně"
                    },
                },
                new Order()
                {
                    Id = Guid.NewGuid(),
                    Note = "Third Order",
                    OrderDate = DateTime.UtcNow.AddDays(-3),
                    DeliveryWeek = 47,
                    PaymentMethod = "Cash on Delivery",
                    ContactPhone = "+192837465",
                    Customer = new Company
                    {
                        Id = Guid.Parse("55d77bd2-f7eb-4387-a008-d212f6aaecb5"),
                        Title = "PRIMA akciová společnost",
                        Ic = "47239743",
                        Dic = "CZ47239743",
                        Street = "Raisova 1004",
                        Psc = "387 47",
                        City = "Strakonice"
                    }
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

    public record Response(
        Guid id,
        string customerName,
        DateTime orderDate,
        int deliveryWeek,
        string paymentMethod,
        string contactPhone,
        string note
    );
}
