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

namespace Fei.Is.Api.Features.Orders.Commands;

public static class CreateOrder
{
    // Record to represent the request for creating an order
    public record Request(int CustomerId, DateTime OrderDate, int DeliveryWeek, string PaymentMethod, string ContactPhone, string? Note);

    // Endpoint definition for handling the creation of orders
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "orders", // Define the route for creating an order
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        // Create a command with the incoming request and user information
                        var command = new Command(request, user);

                        // Send the command to the mediator for handling
                        var result = await mediator.Send(command);

                        // If validation errors occurred, return them as a validation problem
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        // If successful, return a Created response with the order ID
                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateOrder)) // Name the endpoint for easy reference
                .WithTags(nameof(Order)) // Tag the endpoint for categorization in API documentation
                .WithOpenApi(o =>
                {
                    o.Summary = "Create an order"; // Summary for OpenAPI documentation
                    return o;
                });
        }
    }

    // Command to encapsulate the request data and user for creating an order
    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    // Handler to process the command and create an order in the database
    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            // Validate the command using the provided validator
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                // If validation fails, return a ValidationError result
                return Result.Fail(new ValidationError(result));
            }

            // Find the customer in the database by CustomerId
            var customer = await context.Companies.FindAsync(new object[] { message.Request.CustomerId }, cancellationToken);
            // if (customer == null)
            // {
            //     // If customer is not found, return a NotFoundError
            //     return Result.Fail(new NotFoundError("Customer not found"));
            // }

            // Create a new Order entity and populate it with data from the request
            var order = new Order
            {
                OrderDate = DateTime.SpecifyKind(message.Request.OrderDate, DateTimeKind.Utc),
                DeliveryWeek = message.Request.DeliveryWeek,
                PaymentMethod = message.Request.PaymentMethod,
                ContactPhone = message.Request.ContactPhone,
                Note = message.Request.Note,
                Customer = customer // Associate the customer entity
            };

            // Add the new order to the database context
            await context.Orders.AddAsync(order, cancellationToken);
            // Save changes to persist the new order in the database
            await context.SaveChangesAsync(cancellationToken);

            // Return the newly created order's ID as a successful result
            return Result.Ok(order.Id);
        }
    }

    // Validator to ensure that the command contains valid data
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // CustomerId must be greater than 0
            RuleFor(r => r.Request.CustomerId).GreaterThan(0).WithMessage("Customer ID is required and must be greater than 0");
            // OrderDate is required
            RuleFor(r => r.Request.OrderDate).NotEmpty().WithMessage("Order date is required");
            // PaymentMethod is required
            RuleFor(r => r.Request.PaymentMethod).NotEmpty().WithMessage("Payment method is required");
            // ContactPhone is required
            RuleFor(r => r.Request.ContactPhone).NotEmpty().WithMessage("Contact phone is required");
        }
    }
}
