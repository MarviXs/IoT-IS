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

    public record DeviceControlRequest(
        string Name,
        string Color,
        DeviceControlType Type,
        string? RecipeName,
        int Cycles,
        bool IsInfinite,
        string? RecipeOnName,
        string? RecipeOffName,
        string? SensorTag,
        int Order
    );

    public record TemplateRequest(
        string Name,
        List<DeviceCommandRequest> Commands,
        List<SensorRequest> Sensors,
        List<DeviceControlRequest>? Controls = null,
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

            var controlRequests = message.Request.TemplateData.Controls ?? [];
            if (controlRequests.Count > 0)
            {
                var recipeNames = controlRequests
                    .SelectMany(control => new[] { control.RecipeName, control.RecipeOnName, control.RecipeOffName })
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                var recipesByName = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);
                foreach (var recipeName in recipeNames)
                {
                    var recipe = new Recipe { Name = recipeName, DeviceTemplateId = template.Id };
                    await context.Recipes.AddAsync(recipe, cancellationToken);
                    recipesByName[recipeName] = recipe;
                }

                await context.SaveChangesAsync(cancellationToken);

                var sensorsByTag = context
                    .Sensors.Local.Where(sensor => sensor.DeviceTemplateId == template.Id)
                    .ToDictionary(sensor => sensor.Tag, StringComparer.OrdinalIgnoreCase);

                for (var index = 0; index < controlRequests.Count; index++)
                {
                    var controlRequest = controlRequests[index];

                    var recipeId =
                        controlRequest.Type == DeviceControlType.Run
                            ? recipesByName.GetValueOrDefault(controlRequest.RecipeName?.Trim() ?? string.Empty)?.Id
                            : null;
                    var recipeOnId =
                        controlRequest.Type == DeviceControlType.Toggle
                            ? recipesByName.GetValueOrDefault(controlRequest.RecipeOnName?.Trim() ?? string.Empty)?.Id
                            : null;
                    var recipeOffId =
                        controlRequest.Type == DeviceControlType.Toggle
                            ? recipesByName.GetValueOrDefault(controlRequest.RecipeOffName?.Trim() ?? string.Empty)?.Id
                            : null;

                    Sensor? controlSensor = null;
                    if (controlRequest.Type == DeviceControlType.Toggle && !string.IsNullOrWhiteSpace(controlRequest.SensorTag))
                    {
                        sensorsByTag.TryGetValue(controlRequest.SensorTag.Trim(), out controlSensor);
                    }

                    if (controlRequest.Type == DeviceControlType.Run && recipeId == null)
                    {
                        return Result.Fail(new ValidationError(nameof(DeviceControlRequest.RecipeName), "Recipe is required for run controls"));
                    }

                    if (controlRequest.Type == DeviceControlType.Toggle && (recipeOnId == null || recipeOffId == null || controlSensor == null))
                    {
                        var errors = new Dictionary<string, string[]>();
                        if (recipeOnId == null)
                        {
                            errors.Add(nameof(DeviceControlRequest.RecipeOnName), ["On recipe must be defined in controls"]);
                        }

                        if (recipeOffId == null)
                        {
                            errors.Add(nameof(DeviceControlRequest.RecipeOffName), ["Off recipe must be defined in controls"]);
                        }

                        if (controlSensor == null)
                        {
                            errors.Add(nameof(DeviceControlRequest.SensorTag), ["Sensor must be defined in template sensors"]);
                        }

                        return Result.Fail(new ValidationError(errors));
                    }

                    var control = new DeviceControl
                    {
                        Name = controlRequest.Name,
                        Color = controlRequest.Color,
                        Type = controlRequest.Type,
                        RecipeId = controlRequest.Type == DeviceControlType.Run ? recipeId : null,
                        RecipeOnId = controlRequest.Type == DeviceControlType.Toggle ? recipeOnId : null,
                        RecipeOffId = controlRequest.Type == DeviceControlType.Toggle ? recipeOffId : null,
                        SensorId = controlRequest.Type == DeviceControlType.Toggle ? controlSensor?.Id : null,
                        Cycles = controlRequest.Type == DeviceControlType.Run ? controlRequest.Cycles : 1,
                        IsInfinite = controlRequest.Type == DeviceControlType.Run && controlRequest.IsInfinite,
                        Order = controlRequest.Order,
                        DeviceTemplateId = template.Id,
                    };

                    await context.DeviceControls.AddAsync(control, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            return Result.Ok(template.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.TemplateData.Name).NotEmpty().WithMessage("Name is required");
            When(
                x => x.Request.TemplateData.Controls != null,
                () =>
                {
                    RuleForEach(c => c.Request.TemplateData.Controls)
                        .ChildRules(control =>
                        {
                            control.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
                            control.RuleFor(x => x.Color).NotEmpty().WithMessage("Color is required");
                            control.RuleFor(x => x.Type).IsInEnum();
                            control
                                .RuleFor(x => x.RecipeName)
                                .NotEmpty()
                                .When(x => x.Type == DeviceControlType.Run)
                                .WithMessage("Recipe is required for run controls");
                            control
                                .RuleFor(x => x.Cycles)
                                .GreaterThan(0)
                                .When(x => x.Type == DeviceControlType.Run)
                                .WithMessage("Cycles must be greater than 0");
                            control
                                .RuleFor(x => x.IsInfinite)
                                .Equal(false)
                                .When(x => x.Type == DeviceControlType.Toggle)
                                .WithMessage("Toggle controls cannot run infinitely");
                            control
                                .RuleFor(x => x.RecipeOnName)
                                .NotEmpty()
                                .When(x => x.Type == DeviceControlType.Toggle)
                                .WithMessage("On recipe is required for toggle controls");
                            control
                                .RuleFor(x => x.RecipeOffName)
                                .NotEmpty()
                                .When(x => x.Type == DeviceControlType.Toggle)
                                .WithMessage("Off recipe is required for toggle controls");
                            control
                                .RuleFor(x => x.SensorTag)
                                .NotEmpty()
                                .When(x => x.Type == DeviceControlType.Toggle)
                                .WithMessage("Sensor tag is required for toggle controls");
                        });
                }
            );
        }
    }
}
