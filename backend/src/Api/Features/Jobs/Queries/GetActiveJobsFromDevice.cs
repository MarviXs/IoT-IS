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

public static class GetActiveJobsFromDevice
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceAccessToken}/jobs/active",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator, string deviceAccessToken) =>
                    {
                        var query = new Query(deviceAccessToken);
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
                .WithName(nameof(GetActiveJobsFromDevice))
                .AllowAnonymous()
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all active jobs on a device";
                    o.Description = "This endpoint is called by a device to get all active jobs on itself.";
                    return o;
                });
        }
    }

    public record Query(string DeviceId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context.Devices.AsNoTracking().Where(d => d.AccessToken == request.DeviceId).FirstOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var activeJobs = await context
                .Jobs.AsNoTracking()
                .GetActiveJobs(device.Id)
                .Include(j => j.Commands.OrderBy(c => c.Order))
                .ToListAsync(cancellationToken);

            var responses = activeJobs
                .Select(activeJob =>
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
                })
                .ToList();

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
