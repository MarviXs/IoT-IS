using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Scenes.Queries;

public static class GetSceneById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scenes/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(id, user);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetSceneById))
                .WithTags(nameof(Scene))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a scene by id";
                    return o;
                });
        }
    }

    public record Query(Guid SceneId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var scene = await context.Scenes.AsNoTracking().FirstOrDefaultAsync(scene => scene.Id == request.SceneId, cancellationToken);

            if (scene == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (scene.OwnerId != request.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                scene.Name,
                scene.Description,
                scene.IsEnabled,
                scene.Condition,
                scene
                    .Actions.Select(action => new SceneActionResponse(
                        action.Type,
                        action.DeviceId,
                        action.RecipeId,
                        action.NotificationSeverity,
                        action.NotificationMessage
                    ))
                    .ToList(),
                scene.CooldownAfterTriggerTime,
                scene.LastTriggeredAt
            );

            return Result.Ok(response);
        }
    }

    public record SceneActionResponse(
        SceneActionType Type,
        Guid? DeviceId,
        Guid? RecipeId,
        NotificationSeverity? NotificationSeverity,
        string? NotificationMessage
    );

    public record Response(
        string Name,
        string? Description,
        bool IsEnabled,
        string? Condition,
        List<SceneActionResponse> Actions,
        double CooldownAfterTriggerTime = 0,
        DateTimeOffset? LastTriggeredAt = null
    );
}
