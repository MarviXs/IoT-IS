using System.Security.Claims;
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

public static class GetActiveJobs
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/jobs/active",
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
                .WithName(nameof(GetActiveJobs))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all active jobs on a device";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid DeviceId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context.Devices.AsNoTracking().Where(d => d.Id == request.DeviceId).Include(d => d.SharedWithUsers).SingleOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.CanView(request.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var activeJobs = await context
                .Jobs.AsNoTracking()
                .GetActiveJobs(request.DeviceId)
                .Include(j => j.Commands.OrderBy(c => c.Order))
                .ToListAsync(cancellationToken);

            var responses = activeJobs.Select(activeJob =>
            {
                var currentCommand = activeJob.Commands.ElementAtOrDefault(activeJob.CurrentStep - 1)?.DisplayName ?? string.Empty;
                return new Response(
                    activeJob.Id,
                    activeJob.DeviceId,
                    activeJob.Name,
                    activeJob.TotalSteps,
                    activeJob.TotalCycles,
                    activeJob.CurrentStep,
                    activeJob.CurrentCycle,
                    currentCommand,
                    activeJob.Paused,
                    activeJob.GetProgress(),
                    activeJob.Status
                );
            }).ToList();

            return Result.Ok(responses);
        }
    }

    public record Response(
        Guid Id,
        Guid DeviceId,
        string Name,
        int TotalSteps,
        int TotalCycles,
        int CurrentStep,
        int CurrentCycle,
        string CurrentCommand,
        bool Paused,
        double Progress,
        JobStatusEnum Status
    );
}
