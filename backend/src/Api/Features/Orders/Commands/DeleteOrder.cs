using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Orders.Commands;

public static class DeleteOrder
{
    /// <summary>
    /// Defines the endpoint for deleting an order.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "orders/{orderId:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid orderId) =>
                    {
                        var command = new Command(user, orderId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteOrder))
                .WithTags(nameof(Order))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete an order";
                    o.Description = "Deletes an order by its unique identifier.";
                    return o;
                });
        }
    }

    /// <summary>
    /// Command to delete an order.
    /// </summary>
    public record Command(ClaimsPrincipal User, Guid OrderId) : IRequest<Result>;

    /// <summary>
    /// Handles the deletion logic for an order.
    /// </summary>
    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            // Nájdeme objednávku
            var order = await context.Orders
                .Include(o => o.ItemContainers)
                .FirstOrDefaultAsync(o => o.Id == message.OrderId, cancellationToken);

            if (order == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Odstránime závislé záznamy
            context.OrderItemContainers.RemoveRange(order.ItemContainers);

            // Odstránime objednávku
            context.Orders.Remove(order);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    /// <summary>
    /// Validator for the delete order command.
    /// </summary>
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.OrderId)
                .NotEmpty()
                .WithMessage("Order ID is required.");
        }
    }
}
