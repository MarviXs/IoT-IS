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
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Commands;

public static class CreateJob
{
    public record Request(Guid RecipeId, int NoOfReps);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/{deviceId:guid}/jobs",
                    async Task<Results<Created<Guid>, NotFound, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid deviceId
                    ) =>
                    {
                        var command = new Command(request, deviceId, user);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateJob))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a job";
                    o.Description = "Create a job for a device from a recipe.";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var device = await context.Devices.FirstOrDefaultAsync(device => device.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (device.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var recipe = await context
                .Recipes.AsNoTracking()
                .Include(r => r.Steps.OrderBy(s => s.Order))
                .ThenInclude(s => s.Command)
                .Include(r => r.Steps.OrderBy(s => s.Order))
                .ThenInclude(s => s.Subrecipe)
                .FirstOrDefaultAsync(r => r.Id == message.Request.RecipeId, cancellationToken);
            if (recipe == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var job = new Job
            {
                Device = device,
                Name = recipe.Name,
                NoOfReps = message.Request.NoOfReps
            };
            await context.Jobs.AddAsync(job, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            //unpack recipe steps
            var commands = new List<JobCommand>();
            foreach (var step in recipe.Steps.OrderBy(s => s.Order))
            {
                var stepCommands = await UnpackRecipeStep(step, job.Id, 0, 20, cancellationToken);
                commands.AddRange(stepCommands);
            }
            for(int i = 0; i < commands.Count; i++)
            {
                commands[i].Order = i;
            }
            job.NoOfCmds = commands.Count;

            await context.JobCommands.AddRangeAsync(commands, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var jobStatus = new JobStatus
            {
                JobId = job.Id,
                Job = job,
                RetCode = JobStatusEnum.JOB_PENDING,
                Code = JobStatusEnum.JOB_PENDING,
                CurrentStep = 1,
                CurrentCycle = 1,
                TotalSteps = commands.Count
            };
            await context.JobStatuses.AddAsync(jobStatus, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(job.Id);
        }

        private async Task<List<JobCommand>> UnpackRecipeStep(RecipeStep step, Guid jobId, int currentDepth, int maxDepth, CancellationToken cancellationToken)
        {
            var commands = new List<JobCommand>();

            if (currentDepth >= maxDepth)
                return commands;

            if (step.IsCommand)
            {
                var command = new JobCommand
                {
                    JobId = jobId,
                    DisplayName = step.Command!.DisplayName,
                    Name = step.Command.Name,
                    Order = step.Order,
                    Params = step.Command.Params
                };
                commands.Add(command);
            }

            if (step.IsSubRecipe)
            {
                var subrecipe = await context
                    .Recipes.AsNoTracking()
                    .Include(r => r.Steps.OrderBy(s => s.Order))
                    .ThenInclude(s => s.Command)
                    .Include(r => r.Steps.OrderBy(s => s.Order))
                    .ThenInclude(s => s.Subrecipe)
                    .FirstOrDefaultAsync(r => r.Id == step.SubrecipeId, cancellationToken);

                if (subrecipe != null)
                {
                    foreach (var subStep in subrecipe.Steps.OrderBy(s => s.Order))
                    {
                        var subCommands = await UnpackRecipeStep(subStep, jobId, currentDepth + 1, maxDepth, cancellationToken);
                        commands.AddRange(subCommands);
                    }
                }
            }

            return commands;
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.RecipeId).NotEmpty();
            RuleFor(x => x.Request.NoOfReps).GreaterThan(0);
        }
    }
}
