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

public static class UpdateOrder
{
    public record Request(Guid CustomerId, DateTime OrderDate, int DeliveryWeek, string PaymentMethod, string ContactPhone, decimal Discount, string? Note);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "orders/{id}",
                    async Task<Results<NotFound, ValidationProblem, Ok>> (IMediator mediator, ClaimsPrincipal user, Guid id, Request request) =>
                    {
                        var command = new Command(id, request, user);
                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                            return TypedResults.ValidationProblem(result.ToValidationErrors());

                        if (result.IsFailed && result.HasError<NotFoundError>())
                            return TypedResults.NotFound();

                        return TypedResults.Ok();
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

    public record Command(Guid Id, Request Request, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var orderQuery = context.Orders.Where(order => order.Id == message.Id);
            if (!await orderQuery.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            var order = await orderQuery.FirstAsync(cancellationToken);

            var customerQuery = context.Companies.Where(company => company.Id == message.Request.CustomerId);
            if (!await customerQuery.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            var customer = await customerQuery.FirstAsync(cancellationToken);

            order.OrderDate = message.Request.OrderDate;
            order.DeliveryWeek = message.Request.DeliveryWeek;
            order.PaymentMethod = message.Request.PaymentMethod;
            order.ContactPhone = message.Request.ContactPhone;
            order.Note = message.Request.Note;
            order.Customer = customer;
            order.Discount = message.Request.Discount;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.CustomerId).NotEmpty().WithMessage("Customer ID is required");
            RuleFor(r => r.Request.OrderDate).NotEmpty().WithMessage("Order date is required");
            RuleFor(r => r.Request.PaymentMethod).NotEmpty().WithMessage("Payment method is required");
            RuleFor(r => r.Request.ContactPhone).NotEmpty().WithMessage("Contact phone is required");
        }
    }
}
