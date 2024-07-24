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
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Sensors.Commands;

public static class UpdateDeviceTemplateSensors
{
    public record Request(Guid? Id, string Tag, string Name, string? Unit, int? AccuracyDecimals);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "device-templates/{templateId:guid}/sensors",
                    async Task<Results<NoContent, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        List<Request> request,
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

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateDeviceTemplateSensors))
                .WithTags(nameof(Sensor))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update sensors of a device template";
                    o.Description =
                        "Set the sensors of a device template, replacing the existing sensors. If Id is null, the sensor will be created.";
                    return o;
                });
        }
    }

    public record Command(List<Request> SensorRequest, ClaimsPrincipal User, Guid TemplateId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var template = await context
                .DeviceTemplates.Include(template => template.Sensors)
                .FirstOrDefaultAsync(template => template.Id == message.TemplateId, cancellationToken);
            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (template.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            // Delete sensors that are not in the request
            var sensorIds = message.SensorRequest.Select(s => s.Id).ToList();
            var sensorsToDelete = template.Sensors.Where(s => !sensorIds.Contains(s.Id)).ToList();
            context.Sensors.RemoveRange(sensorsToDelete);

            foreach (var sensorRequest in message.SensorRequest)
            {
                var sensor = template.Sensors.FirstOrDefault(s => s.Id == sensorRequest.Id);
                if (sensor == null)
                {
                    sensor = new Sensor
                    {
                        Tag = sensorRequest.Tag,
                        Name = sensorRequest.Name,
                        Unit = sensorRequest.Unit,
                        AccuracyDecimals = sensorRequest.AccuracyDecimals,
                        DeviceTemplateId = template.Id
                    };
                    context.Sensors.Add(sensor);
                }
                else
                {
                    sensor.Tag = sensorRequest.Tag;
                    sensor.Name = sensorRequest.Name;
                    sensor.Unit = sensorRequest.Unit;
                    sensor.AccuracyDecimals = sensorRequest.AccuracyDecimals;
                }
            }

            template.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleForEach(c => c.SensorRequest)
                .ChildRules(sensor =>
                {
                    sensor.RuleFor(s => s.Tag).NotEmpty().WithMessage("Tag is required");

                    sensor.RuleFor(s => s.Name).NotEmpty().WithMessage("Name is required");

                    sensor
                        .RuleFor(s => s.AccuracyDecimals)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("AccuracyDecimals must be greater than or equal to 0");
                });
        }
    }
}
