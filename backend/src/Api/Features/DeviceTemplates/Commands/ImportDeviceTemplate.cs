using System.Linq;
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
using DataCommand = Fei.Is.Api.Data.Models.Command;

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

    public record RecipeStepRequest(string? CommandName, string? SubrecipeName, int Cycles, int Order);

    public record RecipeRequest(string Name, List<RecipeStepRequest>? Steps = null);

    public record TemplateRequest(
        string Name,
        List<DeviceCommandRequest> Commands,
        List<SensorRequest> Sensors,
        List<DeviceControlRequest>? Controls = null,
        List<RecipeRequest>? Recipes = null,
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

            var commandsByName = new Dictionary<string, DataCommand>(StringComparer.OrdinalIgnoreCase);
            foreach (var command in message.Request.TemplateData.Commands)
            {
                var commandName = command.Name.Trim();
                var deviceCommand = new DataCommand
                {
                    DisplayName = command.DisplayName,
                    Name = commandName,
                    Params = command.Params,
                    DeviceTemplateId = template.Id
                };

                await context.Commands.AddAsync(deviceCommand, cancellationToken);
                commandsByName[commandName] = deviceCommand;
            }

            await context.SaveChangesAsync(cancellationToken);

            var sensorsByTag = new Dictionary<string, Sensor>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < message.Request.TemplateData.Sensors.Count; index++)
            {
                var sensor = message.Request.TemplateData.Sensors[index];
                var sensorTag = sensor.Tag.Trim();
                var deviceSensor = new Sensor
                {
                    Tag = sensorTag,
                    Name = sensor.Name,
                    Unit = sensor.Unit,
                    Order = index,
                    AccuracyDecimals = sensor.AccuracyDecimals,
                    DeviceTemplateId = template.Id,
                    Group = sensor.Group
                };

                await context.Sensors.AddAsync(deviceSensor, cancellationToken);
                sensorsByTag[sensorTag] = deviceSensor;
            }

            await context.SaveChangesAsync(cancellationToken);

            var controlRequests = message.Request.TemplateData.Controls ?? [];

            var recipeStepsByName = new Dictionary<string, List<RecipeStepRequest>>(StringComparer.OrdinalIgnoreCase);
            var providedRecipes = message.Request.TemplateData.Recipes ?? [];
            foreach (var recipeRequest in providedRecipes)
            {
                var recipeName = recipeRequest.Name?.Trim();
                if (string.IsNullOrWhiteSpace(recipeName))
                {
                    return Result.Fail(new ValidationError(nameof(TemplateRequest.Recipes), "Recipe name is required"));
                }

                if (recipeStepsByName.ContainsKey(recipeName))
                {
                    return Result.Fail(new ValidationError(nameof(TemplateRequest.Recipes), "Recipe names must be unique"));
                }

                recipeStepsByName[recipeName] = recipeRequest.Steps?.ToList() ?? new List<RecipeStepRequest>();
            }

            var referencedRecipeNames = controlRequests
                .SelectMany(control => new[] { control.RecipeName, control.RecipeOnName, control.RecipeOffName })
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var recipeName in referencedRecipeNames)
            {
                if (!recipeStepsByName.ContainsKey(recipeName))
                {
                    recipeStepsByName[recipeName] = new List<RecipeStepRequest>();
                }
            }

            var recipesByName = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);
            if (recipeStepsByName.Count > 0)
            {
                foreach (var recipeName in recipeStepsByName.Keys)
                {
                    var recipe = new Recipe { Name = recipeName, DeviceTemplateId = template.Id };
                    await context.Recipes.AddAsync(recipe, cancellationToken);
                    recipesByName[recipeName] = recipe;
                }

                await context.SaveChangesAsync(cancellationToken);

                foreach (var (recipeName, steps) in recipeStepsByName)
                {
                    var recipe = recipesByName[recipeName];

                    foreach (var stepRequest in steps)
                    {
                        var commandName = stepRequest.CommandName?.Trim();
                        var subrecipeName = stepRequest.SubrecipeName?.Trim();

                        if (!string.IsNullOrWhiteSpace(commandName) && !string.IsNullOrWhiteSpace(subrecipeName))
                        {
                            return Result.Fail(
                                new ValidationError(nameof(RecipeStepRequest), "Step cannot reference both a command and a subrecipe")
                            );
                        }

                        Guid? commandId = null;
                        if (!string.IsNullOrWhiteSpace(commandName))
                        {
                            if (!commandsByName.TryGetValue(commandName, out var command))
                            {
                                return Result.Fail(
                                    new ValidationError(
                                        nameof(RecipeStepRequest.CommandName),
                                        $"Command '{commandName}' must be defined in the template commands"
                                    )
                                );
                            }

                            commandId = command.Id;
                        }

                        Guid? subrecipeId = null;
                        if (!string.IsNullOrWhiteSpace(subrecipeName))
                        {
                            if (!recipesByName.TryGetValue(subrecipeName, out var subrecipe))
                            {
                                return Result.Fail(
                                    new ValidationError(
                                        nameof(RecipeStepRequest.SubrecipeName),
                                        $"Subrecipe '{subrecipeName}' must be defined in the template"
                                    )
                                );
                            }

                            subrecipeId = subrecipe.Id;
                        }

                        await context.RecipeSteps.AddAsync(
                            new RecipeStep
                            {
                                RecipeId = recipe.Id,
                                CommandId = commandId,
                                SubrecipeId = subrecipeId,
                                Cycles = stepRequest.Cycles,
                                Order = stepRequest.Order
                            },
                            cancellationToken
                        );
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            if (controlRequests.Count > 0)
            {
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
                            errors.Add(nameof(DeviceControlRequest.RecipeOnName), new[] { "On recipe must be defined in controls" });
                        }

                        if (recipeOffId == null)
                        {
                            errors.Add(nameof(DeviceControlRequest.RecipeOffName), new[] { "Off recipe must be defined in controls" });
                        }

                        if (controlSensor == null)
                        {
                            errors.Add(nameof(DeviceControlRequest.SensorTag), new[] { "Sensor must be defined in template sensors" });
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

            When(
                x => x.Request.TemplateData.Recipes != null,
                () =>
                {
                    RuleForEach(x => x.Request.TemplateData.Recipes)
                        .ChildRules(recipe =>
                        {
                            recipe.RuleFor(r => r.Name).NotEmpty().WithMessage("Recipe name is required");
                            recipe
                                .RuleForEach(r => r.Steps ?? new List<RecipeStepRequest>())
                                .ChildRules(step =>
                                {
                                    step.RuleFor(s => s)
                                        .Must(s => string.IsNullOrWhiteSpace(s.CommandName) != string.IsNullOrWhiteSpace(s.SubrecipeName))
                                        .WithMessage("Step must reference either a command or a subrecipe");
                                    step.RuleFor(s => s.Cycles).GreaterThan(0).WithMessage("Cycles must be greater than 0");
                                    step.RuleFor(s => s.Order).GreaterThanOrEqualTo(0).WithMessage("Order must be 0 or greater");
                                });
                        });
                }
            );

            RuleFor(x => x.Request.TemplateData.Recipes)
                .Must(recipes =>
                {
                    if (recipes == null)
                    {
                        return true;
                    }

                    var normalized = recipes
                        .Select(recipe => recipe.Name?.Trim() ?? string.Empty)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();

                    return normalized.Count == normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count();
                })
                .WithMessage("Recipe names must be unique");
        }
    }
}
