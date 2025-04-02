using System.Text.Json;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Jobs.EventHandlers;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Services;

public class JobService(AppDbContext context)
{
    public async Task<Result<Job>> CreateJobFromRecipe(Guid deviceId, Guid recipeId, int cycles, CancellationToken cancellationToken)
    {
        var recipe = await context
            .Recipes.AsNoTracking()
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .ThenInclude(s => s.Command)
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .ThenInclude(s => s.Subrecipe)
            .FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);
        if (recipe == null)
        {
            return Result.Fail(new NotFoundError());
        }

        var job = new Job
        {
            DeviceId = deviceId,
            Name = recipe.Name,
            CurrentStep = 1,
            CurrentCycle = 1,
            TotalCycles = cycles,
            Status = JobStatusEnum.JOB_QUEUED
        };
        await context.Jobs.AddAsync(job, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Unpack recipe steps with cycles
        var commands = new List<JobCommand>();
        foreach (var step in recipe.Steps.OrderBy(s => s.Order))
        {
            var stepCommands = await UnpackRecipeStep(step, job.Id, 0, 20, cancellationToken);
            commands.AddRange(stepCommands);
        }
        for (int i = 0; i < commands.Count; i++)
        {
            commands[i].Order = i;
        }
        job.TotalSteps = commands.Count;
        job.AddDomainEvent(new JobCreatedEvent(job));

        await context.JobCommands.AddRangeAsync(commands, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Ok(job);
    }

    private async Task<List<JobCommand>> UnpackRecipeStep(
        RecipeStep step,
        Guid jobId,
        int currentDepth,
        int maxDepth,
        CancellationToken cancellationToken
    )
    {
        var commands = new List<JobCommand>();

        if (currentDepth >= maxDepth)
            return commands;

        for (int cycle = 0; cycle < step.Cycles; cycle++)
        {
            if (step.IsCommand)
            {
                var command = new JobCommand
                {
                    OriginalCommandId = step.Command!.Id,
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
        }

        return commands;
    }
}
