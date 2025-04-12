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

namespace Fei.Is.Api.Features.OrderItemContainer.Commands
{
    public static class IncreaseQuantityProduct
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost(
                        "orders/{orderId:guid}/container/{containerId:guid}/product/{productId:guid}/increase",
                        async Task<Results<NoContent, NotFound>> (
                            IMediator mediator,
                            ClaimsPrincipal user,
                            Guid orderId,
                            Guid containerId,
                            Guid productId
                        ) =>
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
                    .WithName(nameof(IncreaseQuantityProduct))
                    .WithTags("OrderItem")
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Increase the quantity of a product in a container by 1 and update container pricing";
                        return o;
                    });
            }
        }

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
                // Načítame kontajner vrátane jeho položiek a prislúchajúcich produktov
                var container = await _context
                    .OrderItemContainers.Include(c => c.Items)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(c => c.Id == message.ContainerId, cancellationToken);

                if (container == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Vyhľadáme OrderItem v kontejnere, ktorého priradený produkt má dané ID
                var orderItem = container.Items.FirstOrDefault(oi => oi.Product.Id == message.ProductId);
                if (orderItem == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Zvýšime množstvo objednávkovej položky o 1
                orderItem.Quantity += 1;

                // Uložíme zmeny do databázy
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Ok();
            }
        }
    }
}
