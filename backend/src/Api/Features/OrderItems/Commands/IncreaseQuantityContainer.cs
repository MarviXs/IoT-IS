using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.OrderItemContainer.Commands
{
    public static class IncreaseQuantityContainer
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapPost(
                        "orders/{orderId:guid}/container/{containerId:guid}/increase",
                        async Task<Results<NoContent, NotFound>> (
                            IMediator mediator,
                            ClaimsPrincipal user,
                            Guid orderId,
                            Guid containerId) =>
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
                    .WithName(nameof(IncreaseQuantityContainer))
                    .WithTags(nameof(OrderItemContainer))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Increase the quantity of a container by 1";
                        return o;
                    });
            }
        }

        public record Command(ClaimsPrincipal User, Guid OrderId, Guid ContainerId) : IRequest<Result>;

        public sealed class Handler : IRequestHandler<Command, Result>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
            {
                // Načítame kontajner pomocou ContainerId
                var container = await _context.OrderItemContainers
                    .FirstOrDefaultAsync(c => c.Id == message.ContainerId, cancellationToken);

                if (container == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                // Zvýšime množstvo
                container.Quantity += 1;

                // Uložíme zmeny do databázy
                await _context.SaveChangesAsync(cancellationToken);

                // Reload kontajnera pre aktualizáciu computed vlastností (napr. TotalPrice)
                await _context.Entry(container).ReloadAsync(cancellationToken);

                return Result.Ok();
            }
        }
    }
}
