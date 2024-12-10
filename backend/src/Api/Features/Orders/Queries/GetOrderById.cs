using System.Security.Claims;
using Carter;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Orders.Queries;

public static class GetOrderById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "orders/{id:int}",
                    async Task<Results<NotFound, Ok<Response>>> (IMediator mediator, ClaimsPrincipal user, int id) =>
                    {
                        var query = new Query(user, id);
                        var result = await mediator.Send(query);
                        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetOrderById))
                .WithTags(nameof(Order))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get order by ID";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, int Id) : IRequest<Response?>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query message, CancellationToken cancellationToken)
        {
            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.Customer) // Načítanie priradenej zákazníckej entity
                .Where(o => o.Id == message.Id)
                .Select(order => new Response(
                    order.Id,
                    order.Customer.Title,  // Prístup k názvu zákazníka cez navigačnú vlastnosť
                    order.Customer.Id,
                    order.OrderDate,
                    order.DeliveryWeek,
                    order.PaymentMethod,
                    order.ContactPhone,
                    order.Note
                ))
                .FirstOrDefaultAsync(cancellationToken);

            return order;
        }
    }

    public record Response(int Id, string CustomerName, int CustomerId, DateTime OrderDate, int DeliveryWeek, string PaymentMethod, string ContactPhone, string Note);
}
