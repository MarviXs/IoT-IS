using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
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
                    "orders/{id}",
                    async Task<Results<NotFound, Ok<Response>>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(user, id);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
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

    public record Query(ClaimsPrincipal User, Guid Id) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var order = context
                .Orders.AsNoTracking()
                .Include(o => o.Customer) // Načítanie priradenej zákazníckej entity
                .Where(o => o.Id == message.Id);

            if (!await order.AnyAsync())
            {
                return Result.Fail(new NotFoundError());
            }

            return await order
                .Select(order => new Response(
                    order.Id,
                    order.Customer.Title, // Prístup k názvu zákazníka cez navigačnú vlastnosť
                    order.Customer.Id,
                    order.OrderDate,
                    order.DeliveryWeek,
                    order.PaymentMethod,
                    order.ContactPhone,
                    order.Note
                ))
                .FirstAsync(cancellationToken);
        }
    }

    public record Response(
        Guid Id,
        string CustomerName,
        Guid CustomerId,
        DateTime OrderDate,
        int DeliveryWeek,
        string PaymentMethod,
        string ContactPhone,
        string? Note
    );
}
