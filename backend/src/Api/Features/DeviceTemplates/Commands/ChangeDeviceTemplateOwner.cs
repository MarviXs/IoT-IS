using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Commands;

public static class ChangeDeviceTemplateOwner
{
    public record Request(Guid OwnerId);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "admin/device-templates/{id:guid}/owner",
                    async Task<Results<NoContent, NotFound, ValidationProblem>> (IMediator mediator, Guid id, Request request) =>
                    {
                        var command = new Command(id, request);
                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(ChangeDeviceTemplateOwner))
                .WithTags(nameof(DeviceTemplate))
                .WithOpenApi(o =>
                {
                    o.Summary = "Change device template owner";
                    return o;
                });
        }
    }

    public record Command(Guid TemplateId, Request Request) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var template = await context.DeviceTemplates.FirstOrDefaultAsync(t => t.Id == message.TemplateId, cancellationToken);
            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var newOwner = await context.Users.FirstOrDefaultAsync(u => u.Id == message.Request.OwnerId, cancellationToken);
            if (newOwner == null)
            {
                return Result.Fail(new NotFoundError());
            }

            template.OwnerId = message.Request.OwnerId;
            template.Owner = newOwner;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.TemplateId).NotEmpty().WithMessage("Template ID is required");
            RuleFor(command => command.Request.OwnerId).NotEmpty().WithMessage("Owner ID is required");
        }
    }
}
