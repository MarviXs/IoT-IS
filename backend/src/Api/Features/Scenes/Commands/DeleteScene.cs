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

namespace Fei.Is.Api.Features.Scenes.Commands;

public static class DeleteScene
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "scenes/{id:guid}",
                    async Task<Results<NoContent, NotFound, ProblemHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<BadRequestError>())
                        {
                            return TypedResults.Problem(result.GetError());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteScene))
                .WithTags(nameof(Scene))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a scene";
                    return o;
                });
        }
    }

    public record Command(Guid SceneId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var scene = await context.Scenes.FirstOrDefaultAsync(x => x.Id == message.SceneId, cancellationToken);
            if (scene == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!message.User.IsAdmin() && scene.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }
           
            context.Scenes.Remove(scene);
            await context.SaveChangesAsync(cancellationToken);
            
            return Result.Ok(scene.Id);
        }
    }
}
