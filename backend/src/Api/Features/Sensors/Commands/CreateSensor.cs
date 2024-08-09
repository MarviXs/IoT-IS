using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.Sensors.Commands;

public static class CreateSensor
{
    public record Request(string Tag, string Name, string? Unit, int? AccuracyDecimals);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "device-templates/{templateId:guid}/sensors",
                    async Task<Results<Created<Guid>, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid templateId
                    ) =>
                    {
                        var command = new Command(request, user, templateId);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateSensor))
                .WithTags(nameof(Sensor))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a sensor";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User, Guid TemplateId) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var deviceTemplate = await context.DeviceTemplates.FindAsync([message.TemplateId], cancellationToken: cancellationToken);
            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (deviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var sensor = new Sensor
            {
                DeviceTemplateId = message.TemplateId,
                Tag = message.Request.Tag,
                Name = message.Request.Name,
                Unit = message.Request.Unit,
                AccuracyDecimals = message.Request.AccuracyDecimals
            };

            await context.Sensors.AddAsync(sensor, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(sensor.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.Tag).NotEmpty().MaximumLength(50).WithMessage("Tag is required and must be at most 50 characters long");
            RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(50).WithMessage("Name is required and must be at most 50 characters long");
            RuleFor(command => command.Request.Unit).MaximumLength(50).WithMessage("Unit must be at most 50 characters long");
        }
    }
}
