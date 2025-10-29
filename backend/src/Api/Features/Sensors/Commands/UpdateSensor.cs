using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Sensors.Commands;

public static class UpdateSensor
{
    public record Request(string Tag, string Name, string? Unit, int? AccuracyDecimals, string? Group);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "sensors/{sensorId:guid}",
                    async Task<Results<Ok, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid sensorId
                    ) =>
                    {
                        var command = new Command(request, user, sensorId);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok();
                    }
                )
                .WithName(nameof(UpdateSensor))
                .WithTags(nameof(Sensor))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a sensor";
                    return o;
                });
        }
    }

    public record Command(Request SensorRequest, ClaimsPrincipal User, Guid SensorId) : IRequest<Result>;

    public class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var sensor = await context
                .Sensors.Include(sensor => sensor.DeviceTemplate)
                .FirstOrDefaultAsync(sensor => sensor.Id == message.SensorId, cancellationToken);

            if (sensor == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (sensor.DeviceTemplate == null || !sensor.DeviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            sensor.Tag = message.SensorRequest.Tag;
            sensor.Name = message.SensorRequest.Name;
            sensor.Unit = message.SensorRequest.Unit;
            sensor.AccuracyDecimals = message.SensorRequest.AccuracyDecimals;
            sensor.Group = message.SensorRequest.Group;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.SensorRequest.Tag)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("Tag is required and must be at most 50 characters long");
            RuleFor(command => command.SensorRequest.Name)
                .NotEmpty()
                .MaximumLength(50)
                .WithMessage("Name is required and must be at most 50 characters long");
            RuleFor(command => command.SensorRequest.Unit).MaximumLength(50).WithMessage("Unit must be at most 50 characters long");
        }
    }
}
