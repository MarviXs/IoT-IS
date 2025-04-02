using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Scenes.Utils;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Scenes.Commands;

public static class UpdateScene
{
    public record SceneActionRequest(
        SceneActionType Type,
        Guid? DeviceId,
        Guid? RecipeId,
        NotificationSeverity? NotificationSeverity,
        string? NotificationMessage
    );

    public record Request(
        string Name,
        string? Description,
        bool IsEnabled,
        string? Condition,
        List<SceneActionRequest> Actions,
        double CooldownAfterTriggerTime
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "scenes/{id:guid}",
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request, Guid id) =>
                    {
                        var command = new Command(request, id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(UpdateScene))
                .WithTags(nameof(Scene))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a scene";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid SceneId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var scene = await context.Scenes.FirstOrDefaultAsync(x => x.Id == message.SceneId, cancellationToken: cancellationToken);
            if (scene == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (scene.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            scene.Name = message.Request.Name;
            scene.Description = message.Request.Description;
            scene.IsEnabled = message.Request.IsEnabled;
            scene.Condition = message.Request.Condition;
            scene.CooldownAfterTriggerTime = message.Request.CooldownAfterTriggerTime;
            scene.Actions = message
                .Request.Actions.Select(x => new SceneAction
                {
                    Type = x.Type,
                    DeviceId = x.DeviceId,
                    RecipeId = x.RecipeId,
                    NotificationSeverity = x.NotificationSeverity ?? NotificationSeverity.Info,
                    NotificationMessage = x.NotificationMessage
                })
                .ToList();

            var existingTriggers = context.SceneSensorTriggers.Where(x => x.SceneId == scene.Id).ToList();
            context.SceneSensorTriggers.RemoveRange(existingTriggers);

            var triggers = SceneConditionUtils.ParseCondition(message.Request.Condition).Value;
            foreach (var trigger in triggers)
            {
                var sceneSensorTrigger = new SceneSensorTrigger
                {
                    SceneId = scene.Id,
                    DeviceId = trigger.DeviceId,
                    SensorTag = trigger.Tag
                };
                await context.SceneSensorTriggers.AddAsync(sceneSensorTrigger, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(scene.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
