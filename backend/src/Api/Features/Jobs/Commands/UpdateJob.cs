using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Features.Jobs.Extensions;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Commands;

public static class UpdateJob
{
    public record Request(string Name, int CurrentStep, int TotalSteps, int CurrentCycle, int TotalCycles, bool Paused, JobStatusEnum Status);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "jobs/{jobId:guid}",
                    async Task<Results<NoContent, NotFound, ValidationProblem, ForbidHttpResult>> (IMediator mediator, Guid jobId, Request request) =>
                    {
                        var command = new Command(request, jobId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateJob))
                .WithTags(nameof(Job))
                .AllowAnonymous()
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a job";
                    o.Description = "This endpoint is called by a device to update a job.";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid JobId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator, IHubContext<IsHub, INotificationsClient> hubContext)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var job = await context.Jobs.Include(j => j.Device).FirstOrDefaultAsync(j => j.Id == message.JobId, cancellationToken);

            if (job == null)
            {
                return Result.Fail(new NotFoundError());
            }

            job.Name = message.Request.Name;
            job.CurrentStep = message.Request.CurrentStep;
            job.TotalSteps = message.Request.TotalSteps;
            job.CurrentCycle = message.Request.CurrentCycle;
            job.TotalCycles = message.Request.TotalCycles;
            job.Paused = message.Request.Paused;
            job.Status = message.Request.Status;
            await context.SaveChangesAsync(cancellationToken);

            var jobUpdateDto = new JobUpdateDto(
                job.Id,
                job.DeviceId,
                job.Name,
                job.TotalSteps,
                job.TotalCycles,
                job.CurrentStep,
                job.CurrentCycle,
                job.GetCurrentCommand(),
                job.Paused,
                job.GetProgress(),
                job.Status
            );
            await hubContext.Clients.Group(job.DeviceId.ToString()).ReceiveJobUpdate(jobUpdateDto);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.JobId).NotEmpty().WithMessage("Job ID is required");

            RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Job name is required");

            RuleFor(x => x.Request.CurrentStep).GreaterThanOrEqualTo(0).WithMessage("Current step must be non-negative");

            RuleFor(x => x.Request.TotalSteps).GreaterThan(0).WithMessage("Total steps must be greater than zero");

            RuleFor(x => x.Request.CurrentCycle).GreaterThanOrEqualTo(0).WithMessage("Current cycle must be non-negative");

            RuleFor(x => x.Request.TotalCycles).GreaterThan(0).WithMessage("Total cycles must be greater than zero");
        }
    }
}
