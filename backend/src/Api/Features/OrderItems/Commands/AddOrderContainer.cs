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

namespace Fei.Is.Api.Features.OrderItemContainers.Commands;

public static class AddOrderContainer
{
    // Record to represent the request for adding an order container
    public record Request(Guid OrderId, string Name, int Quantity, decimal PricePerContainer);

    // Endpoint definition for handling the addition of containers to orders
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "orders/{orderId}/container", // Define the route for adding an order container
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Guid orderId, Request request) =>
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

                        // If successful, return a Created response with the order container ID
                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(AddOrderContainer)) // Name the endpoint for easy reference
                .WithTags(nameof(OrderItemContainer)) // Tag the endpoint for categorization in API documentation
                .WithOpenApi(o =>
                {
                    o.Summary = "Add a container to an order"; // Summary for OpenAPI documentation
                    return o;
                });
        }
    }

    // Command to encapsulate the request data and user for adding a container to an order
    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    // Handler to process the command and add the container to the specified order in the database
    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            // Validate the command using the provided validator
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                // If validation fails, return a ValidationError result
                return Result.Fail(new ValidationError(validationResult));
            }

            // Find the order in the database by OrderId
            var orderQuery = context.Orders.Where(order => order.Id == message.Request.OrderId);
            if (!await orderQuery.AnyAsync(cancellationToken))
            {
                // If the order is not found, return a NotFoundError
                return Result.Fail(new NotFoundError());
            }

            // Create a new OrderItemContainer entity and populate it with data from the request
            var orderContainer = new Fei.Is.Api.Data.Models.InformationSystem.OrderItemContainer
            {
                Name = message.Request.Name,
                Quantity = message.Request.Quantity,
            };

            var order = await orderQuery.FirstAsync(cancellationToken);

            order.ItemContainers.Add(orderContainer);
            // Save changes to persist the new order container in the database
            await context.SaveChangesAsync(cancellationToken);

            // Return the newly created order container's ID as a successful result
            return Result.Ok(orderContainer.Id);
        }
    }

    // Validator to ensure that the command contains valid data
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // OrderId is required
            RuleFor(r => r.Request.OrderId).NotEmpty().WithMessage("Order ID is required");
            // Name is required
            RuleFor(r => r.Request.Name).NotEmpty().WithMessage("Container name is required");
            // Quantity must be greater than 0
            RuleFor(r => r.Request.Quantity).GreaterThan(0).WithMessage("Quantity is required and must be greater than 0");
            // PricePerContainer must be greater than or equal to 0
            RuleFor(r => r.Request.PricePerContainer).GreaterThanOrEqualTo(0).WithMessage("Price per container must be greater than or equal to 0");
        }
    }
}
