using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Fei.Is.Api.Data.Models.InformationSystem;
using System.Linq;

namespace Fei.Is.Api.Features.OrderItemContainer.Commands
{
    public static class DecreaseQuantityProduct
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost(
                        "orders/{orderId:guid}/container/{containerId:guid}/product/{productId:guid}/decrease",
                        async Task<Results<NoContent, NotFound>> (IMediator mediator, ClaimsPrincipal user, Guid orderId, Guid containerId, Guid productId) =>
                        {
                            var command = new Command(user, orderId, containerId, productId);
                            var result = await mediator.Send(command);

                            if (result.HasError<NotFoundError>())
                            {
                                return TypedResults.NotFound();
                            }

                            return TypedResults.NoContent();
                        }
                    )
                    .WithName(nameof(DecreaseQuantityProduct))
                    .WithTags("OrderItem")
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Decrease the quantity of a product in a container by 1, remove it if quantity reaches 0, and update container pricing";
                        return o;
                    });
            }
        }

        // Príkazový model obsahuje OrderId, ContainerId a ProductId
        public record Command(ClaimsPrincipal User, Guid OrderId, Guid ContainerId, Guid ProductId) : IRequest<Result>;

        public sealed class Handler : IRequestHandler<Command, Result>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
            {
                // Načítame kontajner vrátane jeho položiek (Items) a prislúchajúcich produktov
                var container = await _context.OrderItemContainers
                    .Include(c => c.Items)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(c => c.Id == message.ContainerId, cancellationToken);

                if (container == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Vyhľadáme objednávkovú položku (OrderItem) v kontejnere, ktorého priradený produkt má dané ID
                var orderItem = container.Items.FirstOrDefault(oi => oi.Product.Id == message.ProductId);
                if (orderItem == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Znížime množstvo o 1
                orderItem.Quantity -= 1;

                // Ak množstvo klesne na 0 alebo menej, odstránime túto položku
                if (orderItem.Quantity <= 0)
                {
                    _context.OrderItems.Remove(orderItem);
                }

                // Prepočítame celkovú kvantitu v kontejnere ako súčet množstiev všetkých položiek
                container.PricePerContainer = container.Items.Sum(oi => oi.Quantity * (oi.Product.PricePerPiecePack ?? 0m));


                // Ak je definovaná cena za jeden kontajner, aktualizujeme celkovú cenu
                if (container.PricePerContainer.HasValue)
                {
                    container.TotalPrice = container.Quantity * container.PricePerContainer.Value;
                }

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Ok();
            }
        }
    }
}
