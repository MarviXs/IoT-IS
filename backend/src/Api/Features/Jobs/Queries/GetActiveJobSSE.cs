using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Jobs.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Jobs.Queries;

public static class GetActiveJobsSSE
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/jobs/active/sse",
                    async Task<Results<Ok, NotFound, ForbidHttpResult>> (
                        HttpContext httpContext,
                        IServiceProvider serviceProvider,
                        ClaimsPrincipal user,
                        Guid deviceId,
                        RedisService redis,
                        CancellationToken ct
                    ) =>
                    {
                        // Create a new scope to ensure services remain available
                        using var scope = serviceProvider.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var response = httpContext.Response;

                        var query = new Query(user, deviceId);
                        var result = await mediator.Send(query, ct);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        response.Headers.Append("Content-Type", "text/event-stream");
                        response.Headers.Append("Cache-Control", "no-cache");
                        response.Headers.Append("X-Accel-Buffering", "no");
                        response.Headers.Append("Connection", "keep-alive");

                        var userId = user.GetUserId();
                        var channel = RedisChannel.Literal($"jobs-active-{userId}");

                        await SendSSEEvent(response, result.Value, ct);
                        var subscriber = redis.GetSubscriber();
                        await subscriber.SubscribeAsync(
                            channel,
                            async (channel, message) =>
                            {
                                using var innerScope = serviceProvider.CreateScope();
                                var mediator = innerScope.ServiceProvider.GetRequiredService<IMediator>();
                                var result = await mediator.Send(query, ct);
                                await SendSSEEvent(response, result.Value, ct);
                            }
                        );

                        try
                        {
                            while (!ct.IsCancellationRequested)
                            {
                                await Task.Delay(Timeout.Infinite, ct);
                            }
                        }
                        finally
                        {
                            await subscriber.UnsubscribeAsync(channel);
                        }

                        return TypedResults.Ok();
                    }
                )
                .WithName(nameof(GetActiveJobsSSE))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all active jobs on a device using Server-Sent Events";
                    return o;
                });
        }

        private static async Task SendSSEEvent(HttpResponse response, List<Response> jobs, CancellationToken ct)
        {
            var data = JsonSerializer.Serialize(jobs);
            await response.WriteAsync($"data: {data}\n\n", ct);
            await response.Body.FlushAsync(ct);
        }
    }

    public record Query(ClaimsPrincipal User, Guid DeviceId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
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

            var activeJobs = await context
                .Jobs.AsNoTracking()
                .GetActiveJobs(request.DeviceId)
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
        Guid id,
        Guid deviceId,
        string name,
        int totalSteps,
        int totalCycles,
        int currentStep,
        int currentCycle,
        string currentCommand,
        bool paused,
        double progress,
        JobStatusEnum status
    );
}
