using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Recipes.Queries;

public static class GetRecipeById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "recipes/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, Guid id, ClaimsPrincipal user) =>
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
                .WithName(nameof(GetRecipeById))
                .WithTags(nameof(Recipe))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a recipe";
                    return o;
                });
        }
    }

    public record Query(Guid RecipeId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var recipe = await context
                .Recipes.AsNoTracking()
                .Include(r => r.Steps.OrderBy(s => s.Order))
                .ThenInclude(s => s.Command)
                .Include(r => r.Steps.OrderBy(s => s.Order))
                .ThenInclude(s => s.Subrecipe)
                .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

            if (recipe == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var stepsResponse = await LoadStepsRecursive(recipe.Steps, 0, 20, cancellationToken);
            var response = new Response(recipe.Id, recipe.Name, stepsResponse);

            return Result.Ok(response);
        }

        private async Task<List<RecipeStepResponse>> LoadStepsRecursive(
            IEnumerable<RecipeStep> steps,
            int currentDepth,
            int maxDepth,
            CancellationToken cancellationToken
        )
        {
            if (currentDepth >= maxDepth)
                return [];

            var stepResponses = new List<RecipeStepResponse>();
            foreach (var step in steps.OrderBy(s => s.Order))
            {
                CommandResponse? commandResponse = null;
                if (step.Command != null)
                {
                    commandResponse = new CommandResponse(step.Command.Id, step.Command.DisplayName, step.Command.Name, step.Command.Params);
                }

                SubrecipeResponse? subrecipeResponse = null;
                if (step.Subrecipe != null)
                {
                    var subSteps = await context
                        .Recipes.Where(r => r.Id == step.SubrecipeId)
                        .SelectMany(r => r.Steps)
                        .Include(s => s.Command)
                        .Include(s => s.Subrecipe)
                        .OrderBy(s => s.Order)
                        .ToListAsync(cancellationToken);

                    subrecipeResponse = new SubrecipeResponse(
                        step.Subrecipe.Id,
                        step.Subrecipe.Name,
                        await LoadStepsRecursive(subSteps, currentDepth + 1, maxDepth, cancellationToken)
                    );
                }

                stepResponses.Add(new RecipeStepResponse(step.Id, commandResponse, subrecipeResponse, step.Cycles, step.Order));
            }

            return stepResponses;
        }
    }

    public record CommandResponse(Guid Id, string DisplayName, string Name, List<double> Params);

    public record SubrecipeResponse(Guid Id, string Name, List<RecipeStepResponse> Steps);

    public record RecipeStepResponse(Guid Id, CommandResponse? Command, SubrecipeResponse? Subrecipe, int Cycles, int Order);

    public record Response(Guid Id, string Name, List<RecipeStepResponse> Steps);
}
