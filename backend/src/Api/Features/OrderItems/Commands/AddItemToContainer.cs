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

namespace Fei.Is.Api.Features.Containers.Commands
{
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
                            Guid orderId,
                            Guid containerId,
                            AddItemRequest request
                        ) =>
                        {
                            // Príkaz teraz obsahuje aj orderId
                            var command = new Command(user, orderId, containerId, request);
                            var result = await mediator.Send(command);

                            if (result.HasError<NotFoundError>())
                            {
                                return TypedResults.NotFound();
                            }

                            // V prípade, že produkt patrí do "Práca", môžeme mať nový kontajner.
                            // Návratová URL používa orderId a productId
                            return TypedResults.Created($"/orders/{command.OrderId}/container/item/{command.Request.ProductId}");
                        }
                    )
                    .WithName(nameof(AddItemToContainer))
                    .WithTags(nameof(OrderItemContainer))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Add an item to an order container";
                        o.Description =
                            "Adds an order item to a specified order container by container ID. " +
                            "If the product's category is 'Práca', a container with name 'Práca' is used (or created) for the order.";
                        return o;
                    });
            }
        }

        public record AddItemRequest(Guid ProductId, int Quantity);

        // Command teraz obsahuje aj OrderId
        public record Command(ClaimsPrincipal User, Guid OrderId, Guid ContainerId, AddItemRequest Request) : IRequest<Result>;

        public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
        {
            public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
            {
                // Načítame produkt vrátane jeho kategórie
                var product = await context
                    .Products.Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == message.Request.ProductId, cancellationToken);

                if (product == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                if (product.Category?.CategoryName == "Práca")
                {
                    // Načítame objednávku spolu s kontajnermi a položkami
                    var order = await context.Orders
                        .Include(o => o.ItemContainers)
                            .ThenInclude(c => c.Items)
                                .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(o => o.Id == message.OrderId, cancellationToken);

                    if (order == null)
                    {
                        return Result.Fail(new NotFoundError());
                    }

                    // Hľadáme existujúci kontajner s názvom "Práca" v objednávke
                    var container = order.ItemContainers.FirstOrDefault(c => c.Name == "Práca");

                    // Ak kontajner neexistuje, vytvoríme ho
                    if (container == null)
                    {
                        container = new Fei.Is.Api.Data.Models.InformationSystem.OrderItemContainer { Name = "Práca", Quantity = 1 };
                        order.ItemContainers.Add(container);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    // Skontrolujeme, či už existuje položka s daným produktom
                    var existingItem = container.Items.FirstOrDefault(i => i.Product.Id == product.Id);
                    if (existingItem != null)
                    {
                        // Zvýšime množstvo
                        existingItem.Quantity += message.Request.Quantity;
                    }
                    else
                    {
                        // Inak pridáme novú položku
                        var orderItem = new OrderItem { Product = product, Quantity = message.Request.Quantity };
                        container.Items.Add(orderItem);
                    }

                    await context.SaveChangesAsync(cancellationToken);

                    // Použijeme asynchrónny reload
                    await context.Entry(container).ReloadAsync(cancellationToken);

                    return Result.Ok();
                }
                else
                {
                    // Iná logika: ak produkt nepatrí do "Práca", použijeme kontajner podľa ID
                    var container = await context.OrderItemContainers
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                        .FirstOrDefaultAsync(c => c.Id == message.ContainerId, cancellationToken);

                    if (container == null)
                    {
                        return Result.Fail(new NotFoundError());
                    }

                    var existingItem = container.Items.FirstOrDefault(i => i.Product.Id == product.Id);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += message.Request.Quantity;
                    }
                    else
                    {
                        var orderItem = new OrderItem { Product = product, Quantity = message.Request.Quantity };
                        container.Items.Add(orderItem);
                    }

                    await context.SaveChangesAsync(cancellationToken);

                    await context.Entry(container).ReloadAsync(cancellationToken);

                    return Result.Ok();
                }
            }
        }

        public sealed class Validator : AbstractValidator<AddItemRequest>
        {
            public Validator()
            {
                RuleFor(r => r.ProductId).NotEmpty().WithMessage("Product ID is required.");
                RuleFor(r => r.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            }
        }
    }
}
