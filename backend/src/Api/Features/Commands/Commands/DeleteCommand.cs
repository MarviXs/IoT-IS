using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
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
                        var commandUsedInRecipesError = result.Errors.OfType<CommandUsedInRecipesError>().FirstOrDefault();

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (commandUsedInRecipesError != null)
                        {
                            return TypedResults.Problem(
                                detail: commandUsedInRecipesError.Message,
                                extensions: new Dictionary<string, object?>
                                {
                                    ["recipeUsageCount"] = commandUsedInRecipesError.RecipeUsageCount,
                                    ["recipeNames"] = commandUsedInRecipesError.RecipeNames
                                }
                            );
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

    public sealed class CommandUsedInRecipesError : BadRequestError
    {
        public CommandUsedInRecipesError(string commandName, IReadOnlyList<string> recipeNames)
            : base(
                $"Cannot delete command \"{commandName}\" because it is used in {(recipeNames.Count == 1 ? "recipe" : "recipes")}: {string.Join(", ", recipeNames)}."
            )
        {
            RecipeNames = recipeNames;
        }

        public int RecipeUsageCount => RecipeNames.Count;

        public IReadOnlyList<string> RecipeNames { get; }
    }

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
            if (command.DeviceTemplate == null || !command.DeviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }
            if (command.RecipeSteps.Any())
            {
                var recipeNames = command.RecipeSteps
                    .Where(step => step.Recipe != null)
                    .Select(step => step.Recipe!.Name.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .Order()
                    .ToArray();

                return Result.Fail(new CommandUsedInRecipesError(command.Name, recipeNames));
            }

            context.Commands.Remove(command);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(command.Id);
        }
    }
}
