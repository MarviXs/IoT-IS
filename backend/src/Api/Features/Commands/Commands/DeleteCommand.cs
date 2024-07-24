using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Commands.Commands;

public static class DeleteCommand
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "commands/{id:guid}",
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
                .WithName(nameof(DeleteCommand))
                .WithTags(nameof(Command))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a command";
                    return o;
                });
        }
    }

    public record Command(Guid CommandId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var command = await context
                .Commands.Include(rs => rs.RecipeSteps)
                .ThenInclude(r => r.Recipe)
                .Include(rs => rs.DeviceTemplate)
                .FirstOrDefaultAsync(x => x.Id == message.CommandId, cancellationToken);
            if (command == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (command.DeviceTemplate == null || command.DeviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }
            if (command.RecipeSteps.Any())
            {
                var firstRecipeStep = command.RecipeSteps.FirstOrDefault();
                if (firstRecipeStep != null && firstRecipeStep.Recipe != null)
                {
                    return Result.Fail(new BadRequestError($"Command {command.Name} is used in recipe {firstRecipeStep.Recipe.Name}"));
                }
                else
                {
                    return Result.Fail(new BadRequestError($"Command {command.Name} is used in an invalid recipe"));
                }
            }

            context.Commands.Remove(command);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(command.Id);
        }
    }
}
