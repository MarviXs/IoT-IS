using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
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
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid jobId) =>
                    {
                        var query = new Query(user, jobId);
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
                .WithName(nameof(GetJobById))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get job";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid JobId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var jobWithOwnerId = await context
                .Jobs.Where(j => j.Id == message.JobId)
                .Include(j => j.Device)
                .Include(j => j.Commands.OrderBy(c => c.Order))
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            var job = jobWithOwnerId;
            var ownerId = jobWithOwnerId?.Device?.OwnerId;

            if (job == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (ownerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var commands = new List<CommandResponse>();
            for (var i = 0; i < job.Commands.Count; i++)
            {
                var command = job.Commands[i];
                var progress = CommandProgress.CommandPending;

                if (job.CurrentStep == i + 1)
                {
                    progress = CommandProgress.CommandProcessing;
                }
                else if (job.CurrentStep > i + 1)
                {
                    progress = CommandProgress.CommandDone;
                }

                commands.Add(new CommandResponse(i + 1, command.Name, progress));
            }

            var response = new Response(
                job.Id,
                job.DeviceId,
                job.Name,
                job.CurrentStep,
                job.TotalSteps,
                job.CurrentCycle,
                job.TotalCycles,
                job.Paused,
                job.GetProgress(),
                job.Status,
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

    public record CommandResponse(int Order, string Name, CommandProgress Progress);

    public record Response(
        Guid Id,
        Guid DeviceId,
        string Name,
        int CurrentStep,
        int TotalSteps,
        int CurrentCycle,
        int TotalCycles,
        bool Paused,
        double Progress,
        JobStatusEnum Status,
        List<CommandResponse> Commands
    );
}
