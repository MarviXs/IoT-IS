using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.RecipeSteps.Commands;

public static class UpdateRecipeSteps
{
    public record Request(Guid? StepId, Guid? CommandId, Guid? SubrecipeId, int Cycles, int Order);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "recipes/{recipeId:guid}/steps",
                    async Task<Results<NoContent, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        List<Request> request,
                        Guid recipeId
                    ) =>
                    {
                        var command = new Command(request, recipeId, user);

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
                .WithName(nameof(UpdateRecipeSteps))
                .WithTags(nameof(RecipeStep))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update recipe with recipe steps";
                    o.Description = "Set the steps of a recipe, replacing the existing steps. If StepId is null, the step will be created.";
                    return o;
                });
        }
    }

    public record Command(List<Request> Request, Guid RecipeId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            // Validate the command
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            // Fetch the recipe along with its steps
            var recipe = await context
                .Recipes.Include(r => r.DeviceTemplate)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == message.RecipeId, cancellationToken);
            if (recipe == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (recipe.DeviceTemplate == null || !recipe.DeviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            // Track existing steps by their ID
            var existingSteps = recipe.Steps!.ToDictionary(s => s.Id);

            // Process each step request
            foreach (var stepRequest in message.Request)
            {
                var errors = new Dictionary<string, string[]>();
                if (stepRequest.CommandId.HasValue && stepRequest.SubrecipeId.HasValue)
                {
                    errors.Add(nameof(stepRequest), ["Step cannot have both CommandId and SubrecipeId"]);
                }
                if (stepRequest.CommandId.HasValue)
                {
                    var command = await context.Commands.FindAsync([stepRequest.CommandId], cancellationToken: cancellationToken);
                    if (command == null)
                    {
                        errors.Add(nameof(stepRequest.CommandId), ["Command not found"]);
                    }
                }
                if (stepRequest.SubrecipeId.HasValue)
                {
                    var subrecipe = await context.Recipes.FindAsync([stepRequest.SubrecipeId], cancellationToken: cancellationToken);
                    if (subrecipe == null)
                    {
                        errors.Add(nameof(stepRequest.SubrecipeId), ["Subrecipe not found"]);
                    }
                }
                if (errors.Count != 0)
                {
                    return Result.Fail(new ValidationError(errors));
                }

                if (stepRequest.StepId.HasValue && existingSteps.TryGetValue(stepRequest.StepId.Value, out var existingStep))
                {
                    // Update existing step
                    existingStep.CommandId = stepRequest.CommandId;
                    existingStep.SubrecipeId = stepRequest.SubrecipeId;
                    existingStep.Cycles = stepRequest.Cycles;
                    existingStep.Order = stepRequest.Order;
                }
                else
                {
                    // Add new step
                    var newStep = new RecipeStep
                    {
                        RecipeId = message.RecipeId,
                        CommandId = stepRequest.CommandId,
                        SubrecipeId = stepRequest.SubrecipeId,
                        Cycles = stepRequest.Cycles,
                        Order = stepRequest.Order
                    };
                    await context.RecipeSteps.AddAsync(newStep, cancellationToken);
                }
            }

            // Remove steps that are not in the request
            var stepIdsInRequest = message.Request.Where(s => s.StepId.HasValue).Select(s => s.StepId.Value).ToHashSet();
            var stepsToRemove = existingSteps.Values.Where(s => !stepIdsInRequest.Contains(s.Id)).ToList();
            context.RecipeSteps.RemoveRange(stepsToRemove);

            await context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleForEach(command => command.Request)
                .Must(step => step.StepId.HasValue || step.CommandId.HasValue || step.SubrecipeId.HasValue)
                .WithMessage("Step must have a StepId, CommandId or SubrecipeId");
        }
    }
}
