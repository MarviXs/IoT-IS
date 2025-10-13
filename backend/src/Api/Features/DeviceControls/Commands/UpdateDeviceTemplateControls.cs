using System.Collections.Generic;
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
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceControls.Commands;

public static class UpdateDeviceTemplateControls
{
    public record Request(
        Guid? Id,
        string Name,
        string Color,
        DeviceControlType Type,
        Guid? RecipeId,
        int Cycles,
        bool IsInfinite,
        Guid? RecipeOnId,
        Guid? RecipeOffId,
        Guid? SensorId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "device-templates/{templateId:guid}/controls",
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
                .WithName(nameof(UpdateDeviceTemplateControls))
                .WithTags(nameof(DeviceControl))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update controls of a device template";
                    o.Description =
                        "Set the controls of a device template, replacing the existing controls. If Id is null, the control will be created.";
                    return o;
                });
        }
    }

    public record Command(List<Request> ControlRequest, ClaimsPrincipal User, Guid TemplateId) : IRequest<Result>;

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
                .DeviceTemplates.Include(t => t.Controls)
                .FirstOrDefaultAsync(t => t.Id == message.TemplateId, cancellationToken);

            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (template.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var validRecipeIds = await context
                .Recipes.Where(r => r.DeviceTemplateId == template.Id)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);
            var recipeLookup = validRecipeIds.ToHashSet();

            var validSensorIds = await context
                .Sensors.Where(s => s.DeviceTemplateId == template.Id)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
            var sensorLookup = validSensorIds.ToHashSet();

            foreach (var control in message.ControlRequest)
            {
                switch (control.Type)
                {
                    case DeviceControlType.Run when !control.RecipeId.HasValue
                        || !recipeLookup.Contains(control.RecipeId.Value):
                    {
                        var failure = new ValidationFailure(
                            nameof(control.RecipeId),
                            "Recipe must belong to the device template"
                        );
                        return Result.Fail(new ValidationError(new ValidationResult([failure])));
                    }
                    case DeviceControlType.Toggle:
                    {
                        if (!control.RecipeOnId.HasValue || !recipeLookup.Contains(control.RecipeOnId.Value))
                        {
                            var failure = new ValidationFailure(
                                nameof(control.RecipeOnId),
                                "On recipe must belong to the device template"
                            );
                            return Result.Fail(new ValidationError(new ValidationResult([failure])));
                        }

                        if (!control.RecipeOffId.HasValue || !recipeLookup.Contains(control.RecipeOffId.Value))
                        {
                            var failure = new ValidationFailure(
                                nameof(control.RecipeOffId),
                                "Off recipe must belong to the device template"
                            );
                            return Result.Fail(new ValidationError(new ValidationResult([failure])));
                        }

                        if (!control.SensorId.HasValue || !sensorLookup.Contains(control.SensorId.Value))
                        {
                            var failure = new ValidationFailure(
                                nameof(control.SensorId),
                                "Sensor must belong to the device template"
                            );
                            return Result.Fail(new ValidationError(new ValidationResult([failure])));
                        }

                        break;
                    }
                }
            }

            context.DeviceControls.RemoveRange(template.Controls);

            for (var index = 0; index < message.ControlRequest.Count; index++)
            {
                var controlRequest = message.ControlRequest[index];
                var control = new DeviceControl
                {
                    Id = controlRequest.Id ?? Guid.NewGuid(),
                    Name = controlRequest.Name,
                    Color = controlRequest.Color,
                    Type = controlRequest.Type,
                    RecipeId = controlRequest.Type == DeviceControlType.Run
                        ? controlRequest.RecipeId
                        : null,
                    RecipeOnId = controlRequest.Type == DeviceControlType.Toggle
                        ? controlRequest.RecipeOnId
                        : null,
                    RecipeOffId = controlRequest.Type == DeviceControlType.Toggle
                        ? controlRequest.RecipeOffId
                        : null,
                    SensorId = controlRequest.Type == DeviceControlType.Toggle
                        ? controlRequest.SensorId
                        : null,
                    Cycles = controlRequest.Type == DeviceControlType.Run
                        ? controlRequest.Cycles
                        : 1,
                    IsInfinite = controlRequest.Type == DeviceControlType.Run && controlRequest.IsInfinite,
                    Order = index,
                    DeviceTemplateId = template.Id,
                };

                context.DeviceControls.Add(control);
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
            RuleForEach(c => c.ControlRequest)
                .ChildRules(control =>
                {
                    control.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
                    control.RuleFor(x => x.Color).NotEmpty().WithMessage("Color is required");
                    control.RuleFor(x => x.Type).IsInEnum();
                    control
                        .RuleFor(x => x.RecipeId)
                        .NotEmpty()
                        .When(x => x.Type == DeviceControlType.Run)
                        .WithMessage("Recipe ID is required");
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
                        .RuleFor(x => x.RecipeOnId)
                        .NotEmpty()
                        .When(x => x.Type == DeviceControlType.Toggle)
                        .WithMessage("On recipe is required for toggle controls");
                    control
                        .RuleFor(x => x.RecipeOffId)
                        .NotEmpty()
                        .When(x => x.Type == DeviceControlType.Toggle)
                        .WithMessage("Off recipe is required for toggle controls");
                    control
                        .RuleFor(x => x.SensorId)
                        .NotEmpty()
                        .When(x => x.Type == DeviceControlType.Toggle)
                        .WithMessage("Sensor is required for toggle controls");
                });
        }
    }
}
