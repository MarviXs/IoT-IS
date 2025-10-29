using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.DeviceTemplates.Commands;

public static class ImportDeviceTemplate
{
    public record DeviceCommandRequest(string DisplayName, string Name, List<double> Params);

    public record SensorRequest(string Tag, string Name, string? Unit, int? AccuracyDecimals, string? Group);

    public record TemplateRequest(
        string Name,
        List<DeviceCommandRequest> Commands,
        List<SensorRequest> Sensors,
        DeviceType DeviceType = DeviceType.Generic,
        bool EnableMap = false,
        bool EnableGrid = false,
        int? GridRowSpan = null,
        int? GridColumnSpan = null,
        bool IsGlobal = false
    );

    public record Request(TemplateRequest TemplateData, string Version);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "device-templates/import",
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        var command = new Command(request, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(ImportDeviceTemplate))
                .WithTags(nameof(DeviceTemplate))
                .WithOpenApi(o =>
                {
                    o.Summary = "Import a device template";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var template = new DeviceTemplate
            {
                OwnerId = message.User.GetUserId(),
                Name = message.Request.TemplateData.Name,
                DeviceType = message.Request.TemplateData.DeviceType,
                IsGlobal = message.User.IsAdmin() && message.Request.TemplateData.IsGlobal,
                EnableMap = message.Request.TemplateData.EnableMap,
                EnableGrid = message.Request.TemplateData.EnableGrid,
                GridRowSpan = message.Request.TemplateData.EnableGrid ? message.Request.TemplateData.GridRowSpan : null,
                GridColumnSpan = message.Request.TemplateData.EnableGrid ? message.Request.TemplateData.GridColumnSpan : null,
            };

            await context.DeviceTemplates.AddAsync(template, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var command in message.Request.TemplateData.Commands)
            {
                var deviceCommand = new Data.Models.Command
                {
                    DisplayName = command.DisplayName,
                    Name = command.Name,
                    Params = command.Params,
                    DeviceTemplateId = template.Id
                };

                await context.Commands.AddAsync(deviceCommand, cancellationToken);
            }

            foreach (var sensor in message.Request.TemplateData.Sensors)
            {
                var deviceSensor = new Sensor
                {
                    Tag = sensor.Tag,
                    Name = sensor.Name,
                    Unit = sensor.Unit,
                    Order = message.Request.TemplateData.Sensors.IndexOf(sensor),
                    AccuracyDecimals = sensor.AccuracyDecimals,
                    DeviceTemplateId = template.Id,
                    Group = sensor.Group
                };

                await context.Sensors.AddAsync(deviceSensor, cancellationToken);
            }
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(template.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.TemplateData.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
