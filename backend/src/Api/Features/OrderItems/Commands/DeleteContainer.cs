using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.OrderItemContainer.Commands;

public static class DeleteContainer
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "orders/{orderId:guid}/container/{containerId:guid}",
                    async Task<Results<NoContent, NotFound>> (IMediator mediator, ClaimsPrincipal user, Guid orderId, Guid containerId) =>
                    {
                        var command = new Command(user, orderId, containerId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteContainer))
                .WithTags(nameof(OrderItemContainer))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a container and its items from an order";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid OrderId, Guid ContainerId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            // Vyhľadanie objednávky s kontajnerom
            var order = await context.Orders
                .Include(o => o.ItemContainers)
                .ThenInclude(ic => ic.Items)
                .FirstOrDefaultAsync(o => o.Id == message.OrderId, cancellationToken);

            if (order == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Vyhľadanie kontajnera podľa ID
            var container = order.ItemContainers.FirstOrDefault(c => c.Id == message.ContainerId);
            if (container == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Odstránenie položiek kontajnera
            context.OrderItems.RemoveRange(container.Items);

            // Odstránenie kontajnera
            context.OrderItemContainers.Remove(container);

            // Uloženie zmien v databáze
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
