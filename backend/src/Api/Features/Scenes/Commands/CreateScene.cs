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

namespace Fei.Is.Api.Features.Scenes.Commands;

public static class CreateScene
{
    public record SceneActionRequest(SceneActionType Type, Guid? DeviceId, Guid? RecipeId, string? NotificationMessage);

    public record Request(string Name, string? Description, bool IsEnabled, string? Condition, List<SceneActionRequest> Actions);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "scenes",
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
                .WithName(nameof(CreateScene))
                .WithTags(nameof(Scene))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a scene";
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

            var createdScene = new Scene
            {
                OwnerId = message.User.GetUserId(),
                Name = message.Request.Name,
                Description = message.Request.Description,
                Condition = message.Request.Condition,
                Actions = message
                    .Request.Actions.Select(x => new SceneAction
                    {
                        Type = x.Type,
                        DeviceId = x.DeviceId,
                        RecipeId = x.RecipeId,
                        NotificationMessage = x.NotificationMessage
                    })
                    .ToList()
            };

            await context.Scenes.AddAsync(createdScene, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(createdScene.Id);
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