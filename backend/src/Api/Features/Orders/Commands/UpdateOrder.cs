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

public static class UpdateOrder
{
    public record Request(
        int CustomerId,
        DateTime OrderDate,
        int DeliveryWeek,
        string PaymentMethod,
        string ContactPhone,
        string? Note
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                "orders/{id:int}",
                async Task<Results<NotFound, ValidationProblem, Ok<int>>> (IMediator mediator, ClaimsPrincipal user, int id, Request request) =>
                {
                    var command = new Command(id, request, user);
                    var result = await mediator.Send(command);

                    if (result.HasError<ValidationError>())
                        return TypedResults.ValidationProblem(result.ToValidationErrors());

                    if (result.IsFailed && result.HasError<NotFoundError>())
                        return TypedResults.NotFound();

                    return TypedResults.Ok(result.Value);
                }
            )
            .WithName(nameof(UpdateOrder))
            .WithTags(nameof(Order))
            .WithOpenApi(o =>
            {
                o.Summary = "Update an order";
                return o;
            });
        }
    }

    public record Command(int Id, Request Request, ClaimsPrincipal User) : IRequest<Result<int>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<int>>
    {
        public async Task<Result<int>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var order = await context.Orders.FindAsync(new object[] { message.Id }, cancellationToken);
            if (order == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var customer = await context.Companies.FindAsync(new object[] { message.Request.CustomerId }, cancellationToken);
            if (customer == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Aktualizácia objednávky
            order.CustomerId = message.Request.CustomerId;
            order.OrderDate = DateTime.SpecifyKind(message.Request.OrderDate, DateTimeKind.Utc);
            order.DeliveryWeek = message.Request.DeliveryWeek;
            order.PaymentMethod = message.Request.PaymentMethod;
            order.ContactPhone = message.Request.ContactPhone;
            order.Note = message.Request.Note;
            order.Customer = customer;

            await context.SaveChangesAsync(cancellationToken);
            return Result.Ok(order.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.CustomerId).GreaterThan(0).WithMessage("Customer ID is required and must be greater than 0");
            RuleFor(r => r.Request.OrderDate).NotEmpty().WithMessage("Order date is required");
            RuleFor(r => r.Request.PaymentMethod).NotEmpty().WithMessage("Payment method is required");
            RuleFor(r => r.Request.ContactPhone).NotEmpty().WithMessage("Contact phone is required");
        }
    }
}
