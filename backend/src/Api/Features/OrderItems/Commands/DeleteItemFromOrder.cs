using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.OrderItems.Commands;

public static class DeleteItemFromOrder
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "orders/{orderId:int}/items/{itemId:int}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid orderId, Guid itemId) =>
                    {
                        var command = new Command(user, orderId, itemId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteItemFromOrder))
                .WithTags(nameof(OrderItem))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete an item from an order";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid OrderId, Guid ItemId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            // Vyhľadáme položku na základe kombinácie OrderId a ItemId
            var orderItem = await context.OrderItems
                .Where(oi => oi.Id == message.OrderId && oi.Id == message.ItemId)
                .FirstOrDefaultAsync(cancellationToken);

            // Ak položka neexistuje, vrátime NotFound
            if (orderItem == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Odstránime konkrétnu položku z databázy
            context.OrderItems.Remove(orderItem);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}

