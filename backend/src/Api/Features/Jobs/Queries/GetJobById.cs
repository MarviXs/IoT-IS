using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Features.Jobs.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Queries;

public static class GetJobById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "jobs/{jobId:guid}",
                    async Task<Results<Ok<Response>, NotFound>> (IMediator mediator, Guid jobId) =>
                    {
                        var query = new Query(jobId);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(GetJobById))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get job";
                    return o;
                });
        }
    }

    public record Query(Guid JobId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var job = await context
                .Jobs.Where(j => j.Id == message.JobId)
                .Include(j => j.Device)
                .Include(j => j.Commands.OrderBy(c => c.Order))
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            if (job == null)
            {
                return Result.Fail(new NotFoundError());
            }
            var commands = new List<CommandResponse>();
            for (var i = 0; i < job.Commands.Count; i++)
            {
                var command = job.Commands[i];
                var progress = CommandProgress.CommandPending;

                if (job.Status == JobStatusEnum.JOB_SUCCEEDED)
                {
                    progress = CommandProgress.CommandDone;
                }
                else if (job.Status == JobStatusEnum.JOB_CANCELED || job.Status == JobStatusEnum.JOB_FAILED)
                {
                    if (job.CurrentStep > i + 1)
                    {
                        progress = CommandProgress.CommandDone;
                    }
                    else
                    {
                        progress = CommandProgress.CommandPending;
                    }
                }
                else if (job.CurrentStep == i + 1)
                {
                    progress = CommandProgress.CommandProcessing;
                }
                else if (job.CurrentStep > i + 1)
                {
                    progress = CommandProgress.CommandDone;
                }

                commands.Add(new CommandResponse(i + 1, command.Name, progress, command.Params));
            }

            var response = new Response(
                job.Id,
                job.DeviceId,
                job.Name,
                job.CurrentStep,
                job.TotalSteps,
            job.CurrentCycle,
            job.TotalCycles,
            job.GetCurrentCommand(),
            job.Paused,
            job.IsInfinite,
            job.GetProgress(),
            job.Status,
            job.StartedAt,
            job.FinishedAt,
            commands
        );

        return Result.Ok(response);
    }
}

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommandProgress
    {
        CommandPending = 0,
        CommandProcessing = 1,
        CommandDone = 2
    }

    public record CommandResponse(int Order, string Name, CommandProgress Progress, List<double> Params);

    public record Response(
        Guid Id,
        Guid DeviceId,
        string Name,
        int CurrentStep,
        int TotalSteps,
        int CurrentCycle,
        int TotalCycles,
        string CurrentCommand,
        bool Paused,
        bool IsInfinite,
        double Progress,
        JobStatusEnum Status,
        DateTime? StartedAt,
        DateTime? FinishedAt,
        List<CommandResponse> Commands
    );
}
