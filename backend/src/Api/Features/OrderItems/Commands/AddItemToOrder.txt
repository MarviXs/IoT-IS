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

namespace Fei.Is.Api.Features.OrderItems.Commands;

public static class AddItemToOrder
{
    // Record to represent the request for adding an item to an order
    public record Request(
        int OrderId,
        Guid ProductNumber,
        string VarietyName,
        int Quantity
    );

    // Endpoint definition for handling the addition of items to orders
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "orders/{orderId}/items", // Define the route for adding an item to an order
                    async Task<Results<Created<int>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, int orderId, Request request) =>
                    {
                        // Create a command with the incoming request and user information
                        var command = new Command(request with { OrderId = orderId }, user);

                        // Send the command to the mediator for handling
                        var result = await mediator.Send(command);

                        // If validation errors occurred, return them as a validation problem
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        // If successful, return a Created response with the order item ID
                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(AddItemToOrder)) // Name the endpoint for easy reference
                .WithTags(nameof(OrderItem)) // Tag the endpoint for categorization in API documentation
                .WithOpenApi(o =>
                {
                    o.Summary = "Add an item to an order"; // Summary for OpenAPI documentation
                    return o;
                });
        }
    }

    // Command to encapsulate the request data and user for adding an item to an order
    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<int>>;

    // Handler to process the command and add the item to the specified order in the database
    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command message, CancellationToken cancellationToken)
        {
            // Validate the command using the provided validator
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                // If validation fails, return a ValidationError result
                return Result.Fail(new ValidationError(validationResult));
            }

            // Find the order in the database by OrderId
            var order = await context.Orders.FindAsync(new object[] { message.Request.OrderId }, cancellationToken);
            if (order == null)
            {
                // If the order is not found, return a NotFoundError
                return Result.Fail(new NotFoundError());
            }

            // Create a new OrderItem entity and populate it with data from the request
            var orderItem = new OrderItem
            {
                OrderId = message.Request.OrderId,
                ProductNumber = message.Request.ProductNumber,
                VarietyName = message.Request.VarietyName,
                Quantity = message.Request.Quantity,
                Order = order // Associate the order entity
            };

            // Add the new order item to the database context
            await context.OrderItems.AddAsync(orderItem, cancellationToken);
            // Save changes to persist the new order item in the database
            await context.SaveChangesAsync(cancellationToken);

            // Return the newly created order item's ID as a successful result
            return Result.Ok(orderItem.Id);
        }
    }

    // Validator to ensure that the command contains valid data
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // OrderId must be greater than 0
            RuleFor(r => r.Request.OrderId).GreaterThan(0).WithMessage("Order ID is required and must be greater than 0");
            // ProductNumber is required
            RuleFor(r => r.Request.ProductNumber).NotEmpty().WithMessage("Product number is required");
            // VarietyName is required
            RuleFor(r => r.Request.VarietyName).NotEmpty().WithMessage("Variety name is required");
            // Quantity must be greater than 0
            RuleFor(r => r.Request.Quantity).GreaterThan(0).WithMessage("Quantity is required and must be greater than 0");
        }
    }
}
