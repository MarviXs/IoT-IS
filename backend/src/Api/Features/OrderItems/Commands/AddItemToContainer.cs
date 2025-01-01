using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Containers.Commands;

public static class AddItemToContainer
{
    /// <summary>
    /// Endpoint pre pridanie položky do kontajnera.
    /// </summary>
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "orders/{orderId}/container/{containerId}/item",
                    async Task<Results<Created, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid containerId,
                        AddItemRequest request) =>
                    {
                        var command = new Command(user, containerId, request);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        
                        return TypedResults.Created($"/orders/{command.ContainerId}/item/{command.Request.ProductId}");
                    }
                )
                .WithName(nameof(AddItemToContainer))
                .WithTags(nameof(OrderItemContainer))
                .WithOpenApi(o =>
                {
                    o.Summary = "Add an item to an order container";
                    o.Description = "Adds an order item to a specified order container by container ID.";
                    return o;
                });
        }
    }

    /// <summary>
    /// DTO pre požiadavku na pridanie položky.
    /// </summary>
    public record AddItemRequest(
        Guid ProductId,
        int Quantity
    );

    /// <summary>
    /// Command pre pridanie položky do kontajnera.
    /// </summary>
    public record Command(ClaimsPrincipal User, Guid ContainerId, AddItemRequest Request) : IRequest<Result>;

    /// <summary>
    /// Handler pre spracovanie pridania položky do kontajnera.
    /// </summary>
    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
        
            // Vyhľadaj kontajner
            var container = await context.OrderItemContainers
                .Include(c => c.Items) // Predpokladám, že kontajner má kolekciu položiek
                .FirstOrDefaultAsync(c => c.Id == message.ContainerId, cancellationToken);

            if (container == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Vyhľadaj produkt
            var product = await context.Products
                .FirstOrDefaultAsync(p => p.Id == message.Request.ProductId, cancellationToken);

            if (product == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Vytvor novú položku objednávky
            var orderItem = new OrderItem
            {
                Product = product,
                Quantity = message.Request.Quantity
            };

            // Pridaj položku do kontajnera
            container.Items.Add(orderItem);

            // Ulož zmeny
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    /// <summary>
    /// Validator pre požiadavku na pridanie položky.
    /// </summary>
    public sealed class Validator : AbstractValidator<AddItemRequest>
    {
        public Validator()
        {
            RuleFor(r => r.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(r => r.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
