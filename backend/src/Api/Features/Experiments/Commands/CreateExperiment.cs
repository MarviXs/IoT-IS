using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Features.Jobs.Services;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Experiments.Commands;

public static class CreateExperiment
{
    public record Request(
        string? Note,
        Guid? RecipeToRunId,
        Guid? DeviceId,
        bool AllowCreateWithoutMatchedJob = false,
        int Cycles = 1,
        bool IsInfinite = false,
        DateTime? StartedAt = null,
        DateTime? FinishedAt = null
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "experiments",
                    async Task<Results<Created<Guid>, ValidationProblem, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request
                    ) =>
                    {
                        var command = new Command(request, user);
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

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateExperiment))
                .WithTags(nameof(Experiment))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create an experiment";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, JobService jobService, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            Job? job = null;
            Device? device = null;

            if (message.Request.DeviceId is Guid deviceId)
            {
                device = await context.Devices.Include(d => d.SharedWithUsers).FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);
                if (device == null)
                {
                    return Result.Fail(new NotFoundError());
                }
                if (!device.CanEdit(message.User))
                {
                    return Result.Fail(new ForbiddenError());
                }
            }

            if (message.Request.RecipeToRunId is Guid recipeId)
            {
                if (device == null)
                {
                    return Result.Fail(new ValidationError("DeviceId", "Device ID is required when a recipe is provided."));
                }

                var jobResult = await jobService.CreateJobFromRecipe(
                    device.Id,
                    recipeId,
                    message.Request.Cycles,
                    message.Request.IsInfinite,
                    cancellationToken
                );

                if (jobResult.IsFailed)
                {
                    return Result.Fail(jobResult.Errors);
                }

                job = jobResult.Value;
            }
            else if (device != null && message.Request.StartedAt.HasValue && message.Request.FinishedAt.HasValue)
            {
                var startedAt = message.Request.StartedAt.Value;
                var finishedAt = message.Request.FinishedAt.Value;
                job = new Job
                {
                    DeviceId = device.Id,
                    Name = $"Manual {startedAt:yyyy-MM-dd HH:mm:ss}",
                    Status = JobStatusEnum.JOB_SUCCEEDED,
                    CurrentStep = 0,
                    TotalSteps = 0,
                    CurrentCycle = 1,
                    TotalCycles = 1,
                    StartedAt = startedAt,
                    FinishedAt = finishedAt
                };

                await context.Jobs.AddAsync(job, cancellationToken);
            }

            var experiment = new Experiment
            {
                OwnerId = message.User.GetUserId(),
                DeviceId = device?.Id,
                Note = message.Request.Note,
                RecipeToRunId = message.Request.RecipeToRunId,
                RanJobId = job?.Id,
                StartedAt = job?.StartedAt ?? message.Request.StartedAt ?? DateTime.UtcNow,
                FinishedAt = job?.FinishedAt ?? message.Request.FinishedAt
            };

            await context.Experiments.AddAsync(experiment, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(experiment.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Note).MaximumLength(1024);

            When(
                x => x.Request.RecipeToRunId.HasValue,
                () =>
                {
                    RuleFor(x => x.Request.DeviceId).NotEmpty().WithMessage("Device ID is required when a recipe is provided");
                    RuleFor(x => x.Request.Cycles).GreaterThan(0).WithMessage("Cycles must be greater than 0");
                }
            );
            When(
                x => !x.Request.RecipeToRunId.HasValue,
                () =>
                {
                    RuleFor(x => x.Request.DeviceId).NotEmpty().WithMessage("Device ID is required in manual mode");
                    RuleFor(x => x.Request.StartedAt).NotEmpty().WithMessage("StartedAt is required in manual mode");
                    RuleFor(x => x.Request.FinishedAt).NotEmpty().WithMessage("FinishedAt is required in manual mode");
                    RuleFor(x => x.Request).Must(x => !x.StartedAt.HasValue || !x.FinishedAt.HasValue || x.StartedAt <= x.FinishedAt)
                        .WithMessage("StartedAt must be before or equal to FinishedAt");
                }
            );
        }
    }
}
