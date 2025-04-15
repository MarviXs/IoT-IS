using System.Linq;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.OrderItems.Commands
{
    public static class DeleteItemFromOrder
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapDelete(
                        "orders/{orderId:guid}/items/{itemId:guid}",
                        async Task<Results<NoContent, NotFound, ForbidHttpResult>> (
                            IMediator mediator,
                            ClaimsPrincipal user,
                            Guid orderId,
                            Guid itemId) =>
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
                    .WithTags("OrderItem")
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Delete an item from an order";
                        return o;
                    });
            }
        }

        public record Command(ClaimsPrincipal User, Guid OrderId, Guid ItemId) : IRequest<Result>;

        public sealed class Handler : IRequestHandler<Command, Result>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
            {
                // Načítame kontajner obsahujúci položku (OrderItem) s daným ID
                var container = await _context.OrderItemContainers
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Items.Any(i => i.Id == message.ItemId), cancellationToken);

                if (container == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Nájdeme objednávkovú položku v rámci tohto kontajnera
                var orderItem = container.Items.FirstOrDefault(i => i.Id == message.ItemId);
                if (orderItem == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Odstránime položku z kolekcie kontajnera
                container.Items.Remove(orderItem);

                // Uložíme zmeny do databázy
                await _context.SaveChangesAsync(cancellationToken);

                // Reload kontajnera pre aktualizáciu computed vlastností (napr. TotalPrice)
                await _context.Entry(container).ReloadAsync(cancellationToken);

                return Result.Ok();
            }
        }
    }
}
