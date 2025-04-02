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

public static class EnableScene
{
    public record Request(bool IsEnabled);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(
                    "scenes/{id:guid}/enable",
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
                .WithName(nameof(EnableScene))
                .WithTags(nameof(Scene))
                .WithOpenApi(o =>
                {
                    o.Summary = "Enable or disable a scene";
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

            scene.IsEnabled = message.Request.IsEnabled;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(scene.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.IsEnabled).NotNull();
        }
    }
}
