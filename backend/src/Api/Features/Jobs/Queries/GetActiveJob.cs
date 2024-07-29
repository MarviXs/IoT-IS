using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Queries;

public static class GetActiveJob
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/jobs/active",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid deviceId) =>
                    {
                        var query = new Query(user, deviceId);
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
                .WithName(nameof(GetActiveJob))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get the active job on a device";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid DeviceId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context.Devices.AsNoTracking().Where(d => d.Id == request.DeviceId).SingleOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (device.OwnerId != request.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var activeJob = await context
                .Jobs.AsNoTracking()
                .Where(j => j.DeviceId == request.DeviceId)
                .Include(j => j.Status)
                .Include(j => j.Commands.OrderBy(c => c.Order))
                .Where(
                    j =>
                        j.Status!.Code == JobStatusEnum.JOB_PROCESSING
                        || j.Status.Code == JobStatusEnum.JOB_IDLE
                        || j.Status.Code == JobStatusEnum.JOB_PENDING
                        || j.Status.Code == JobStatusEnum.JOB_PAUSED
                        || j.Status.Code == JobStatusEnum.JOB_CANCELED
                )
                .SingleOrDefaultAsync(cancellationToken);

            if (activeJob == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var currentCommand = activeJob.Commands.ElementAtOrDefault((activeJob.Status?.CurrentStep ?? 1) - 1)?.Name ?? string.Empty;
            var response = new Response(
                activeJob.Id,
                activeJob.Name,
                activeJob.NoOfCmds,
                activeJob.NoOfReps,
                activeJob.Status!.CurrentStep,
                activeJob.Status.CurrentCycle,
                currentCommand,
                activeJob.ToCancel,
                activeJob.Paused,
                activeJob.GetProgress(),
                activeJob.Status.Code
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        string Name,
        int NoOfCmds,
        int NoOfReps,
        int CurrentStep,
        int CurrentCycle,
        string CurrentCommand,
        bool ToCancel,
        bool Paused,
        double Progress,
        JobStatusEnum Status
    );
}
