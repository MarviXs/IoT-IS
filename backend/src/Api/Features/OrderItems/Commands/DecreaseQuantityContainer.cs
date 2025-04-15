using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.OrderItemContainer.Commands;

public static class DecreaseQuantityContainer
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "orders/{orderId:guid}/container/{containerId:guid}/decrease",
                    async Task<Results<NoContent, NotFound, BadRequest>> (
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

                        if (result.HasError<BadRequestError>())
                        {
                            return TypedResults.BadRequest();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DecreaseQuantityContainer))
                .WithTags(nameof(OrderItemContainer))
                .WithOpenApi(o =>
                {
                    o.Summary = "Decrease the quantity of a container by 1";
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
            // Načítame kontajner priamo pomocou ContainerId
            var container = await _context.OrderItemContainers
                .FirstOrDefaultAsync(c => c.Id == message.ContainerId, cancellationToken);

            if (container == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Zabezpečíme, aby množstvo nekleslo pod 0
            if (container.Quantity <= 0)
            {
                return Result.Fail(new BadRequestError("Quantity cannot be decreased below 0."));
            }

            // Zníženie množstva
            container.Quantity -= 1;

            // Uložíme zmeny do databázy
            await _context.SaveChangesAsync(cancellationToken);

            // Reload kontajnera pre aktualizáciu computed vlastností (napr. TotalPrice)
            await _context.Entry(container).ReloadAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
