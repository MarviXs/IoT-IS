using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
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

public class JobStatusReceived(AppDbContext appContext, RedisService redis, IHubContext<IsHub, INotificationsClient> hubContext)
{
    public async Task<Result> Handle(string deviceAccessToken, ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        var redisKey = $"device:{deviceAccessToken}:id";
        var cachedDeviceId = await redis.Db.StringGetAsync(redisKey);

        string deviceId;

        if (cachedDeviceId.HasValue)
        {
            deviceId = cachedDeviceId!;
        }
        else
        {
            var deviceGuid = await appContext
                .Devices.Where(d => d.AccessToken == deviceAccessToken)
                .Select(d => d.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (deviceGuid == Guid.Empty)
            {
                return Result.Fail("Device not found");
            }
            deviceId = deviceGuid.ToString();

            // Store deviceId in Redis cache
            await redis.Db.StringSetAsync(redisKey, deviceId, TimeSpan.FromHours(1));
        }

        redis.Db.StringSetAsync($"device:{deviceId}:connected", "1", TimeSpan.FromMinutes(30));
        redis.Db.StringSetAsync($"device:{deviceId}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

        var jobFbs = JobFbs.GetRootAsJobFbs(new ByteBuffer(payload.ToArray()));

        var job = await appContext
            .Jobs.Where(j => j.DeviceId == Guid.Parse(deviceId) && j.Id == Guid.Parse(jobFbs.JobId))
            .Include(j => j.Device)
            .Include(j => j.Commands.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return Result.Fail(new NotFoundError());
        }

        job.Status = (JobStatusEnum)jobFbs.Status;
        job.CurrentStep = jobFbs.CurrentStep;
        job.TotalSteps = jobFbs.TotalSteps;
        job.CurrentCycle = jobFbs.CurrentCycle;
        job.TotalCycles = jobFbs.TotalCycles;
        job.Paused = jobFbs.Paused;
        job.StartedAt = DateTimeOffset.FromUnixTimeMilliseconds(jobFbs.StartedAt).UtcDateTime;
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
            job.GetProgress(),
            job.Status
        );

        await hubContext.Clients.Group(job.DeviceId.ToString()).ReceiveJobUpdate(jobUpdate);

        return Result.Ok();
    }
}
