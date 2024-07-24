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

public static class GetJobsOnDevice
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/jobs",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid deviceId) =>
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
                .WithName(nameof(GetJobsOnDevice))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all jobs on a device";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid DeviceId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var deviceWithJobs = await context
                .Devices.AsNoTracking()
                .Where(d => d.Id == request.DeviceId)
                .Select(
                    d =>
                        new
                        {
                            d.OwnerId,
                            Jobs = d.Jobs.Select(
                                job =>
                                    new Response(
                                        job.Id,
                                        job.Name,
                                        job.NoOfCmds,
                                        job.NoOfReps,
                                        job.ToCancel,
                                        job.Paused,
                                        job.StartedAt,
                                        job.FinishedAt,
                                        job.CreatedAt,
                                        job.UpdatedAt,
                                        job.Status != null
                                            ? new JobStatusResponse(
                                                job.Status.RetCode,
                                                job.Status.Code,
                                                job.Status.CurrentStep,
                                                job.Status.TotalSteps,
                                                job.Status.CurrentCycle
                                            )
                                            : null
                                    )
                            )
                                .ToList()
                        }
                )
                .FirstOrDefaultAsync(cancellationToken);

            if (deviceWithJobs == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (deviceWithJobs.OwnerId != request.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            return Result.Ok(deviceWithJobs.Jobs);
        }
    }

    public record JobStatusResponse(JobStatusEnum RetCode, JobStatusEnum Code, int CurrentStep, int TotalSteps, int CurrentCycle);

    public record Response(
        Guid Id,
        string Name,
        int NoOfCmds = 0,
        int NoOfReps = 0,
        bool ToCancel = false,
        bool Paused = false,
        DateTime? StartedAt = null,
        DateTime? FinishedAt = null,
        DateTime? CreatedAt = null,
        DateTime? UpdatedAt = null,
        JobStatusResponse? Status = null
    );
}
