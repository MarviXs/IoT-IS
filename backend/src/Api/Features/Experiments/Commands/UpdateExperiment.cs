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

namespace Fei.Is.Api.Features.Experiments.Commands;

public static class UpdateExperiment
{
    public record Request(string? Note, Guid? RecipeToRunId, Guid? RanJobId, DateTime? StartedAt, DateTime? FinishedAt);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "experiments/{id:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid id
                    ) =>
                    {
                        var command = new Command(request, id, user);
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
                .WithName(nameof(UpdateExperiment))
                .WithTags(nameof(Experiment))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update an experiment";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid ExperimentId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var experiment = await context.Experiments.FirstOrDefaultAsync(e => e.Id == message.ExperimentId, cancellationToken);
            if (experiment == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!message.User.IsAdmin() && experiment.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            if (message.Request.RecipeToRunId is Guid recipeId)
            {
                var recipeExists = await context.Recipes.AnyAsync(r => r.Id == recipeId, cancellationToken);
                if (!recipeExists)
                {
                    return Result.Fail(new NotFoundError());
                }
            }

            if (message.Request.RanJobId is Guid jobId)
            {
                var jobExists = await context.Jobs.AnyAsync(j => j.Id == jobId, cancellationToken);
                if (!jobExists)
                {
                    return Result.Fail(new NotFoundError());
                }
            }

            experiment.Note = message.Request.Note;
            experiment.RecipeToRunId = message.Request.RecipeToRunId;
            experiment.RanJobId = message.Request.RanJobId;
            experiment.StartedAt = message.Request.StartedAt;
            experiment.FinishedAt = message.Request.FinishedAt;

            await context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Note).MaximumLength(1024);
        }
    }
}
