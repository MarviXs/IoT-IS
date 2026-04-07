using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Devices.Services;
using Fei.Is.Api.Features.Jobs.Extensions;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using FluentResults;
using Google.FlatBuffers;
using JobFlatBuffers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.MqttClient.Subscribe;

public class JobStatusReceived(
    AppDbContext appContext,
    DeviceAccessTokenResolver deviceAccessTokenResolver,
    RedisService redis,
    IHubContext<IsHub, INotificationsClient> hubContext
)
{
    public async Task<Result> Handle(string deviceAccessToken, ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        var deviceId = await deviceAccessTokenResolver.ResolveDeviceIdAsync(deviceAccessToken, cancellationToken);
        if (!deviceId.HasValue)
        {
            return Result.Fail("Device not found");
        }
        var deviceIdString = deviceId.Value.ToString();

        redis.Db.StringSetAsync($"device:{deviceIdString}:connected", "1", TimeSpan.FromMinutes(30));
        redis.Db.StringSetAsync($"device:{deviceIdString}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

        var jobFbs = JobFbs.GetRootAsJobFbs(new ByteBuffer(payload.ToArray()));

        var job = await appContext
            .Jobs.Where(j => j.DeviceId == deviceId.Value && j.Id == Guid.Parse(jobFbs.JobId))
            .Include(j => j.Device)
            .Include(j => j.Commands.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return Result.Fail(new NotFoundError());
        }

        if (jobFbs.StartedAt != 0)
        {
            job.StartedAt = DateTimeOffset.FromUnixTimeMilliseconds(jobFbs.StartedAt).UtcDateTime;
        }
        else if (job.Status == JobStatusEnum.JOB_QUEUED && (JobStatusEnum)jobFbs.Status != JobStatusEnum.JOB_QUEUED)
        {
            job.StartedAt = DateTime.UtcNow;
        }

        job.Status = (JobStatusEnum)jobFbs.Status;
        job.CurrentStep = jobFbs.CurrentStep;
        job.TotalSteps = jobFbs.TotalSteps;
        job.CurrentCycle = jobFbs.CurrentCycle;
        job.TotalCycles = jobFbs.TotalCycles;
        job.Paused = jobFbs.Paused;

        job.FinishedAt = DateTimeOffset.FromUnixTimeMilliseconds(jobFbs.FinishedAt).UtcDateTime;


        await appContext.SaveChangesAsync(cancellationToken);

        var jobUpdate = new JobUpdateDto(
            job.Id,
            job.DeviceId,
            job.Name,
            job.TotalSteps,
            job.TotalCycles,
            job.CurrentStep,
            job.CurrentCycle,
            job.GetCurrentCommand(),
            job.Paused,
            job.IsInfinite,
            job.GetProgress(),
            job.Status
        );

        await hubContext.Clients.Group(job.DeviceId.ToString()).ReceiveJobUpdate(jobUpdate);

        return Result.Ok();
    }
}
