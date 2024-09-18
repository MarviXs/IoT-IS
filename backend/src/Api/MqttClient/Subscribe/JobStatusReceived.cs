using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using FluentResults;
using Google.FlatBuffers;
using JobFlatBuffers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.MqttClient.Subscribe;

public class JobStatusReceived(AppDbContext appContext, RedisService redis)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return Result.Fail(new NotFoundError());
        }

        job.Status = (Data.Enums.JobStatusEnum)jobFbs.Status;
        job.CurrentStep = jobFbs.CurrentStep;
        job.TotalSteps = jobFbs.TotalSteps;
        job.CurrentCycle = jobFbs.CurrentCycle;
        job.TotalCycles = jobFbs.TotalCycles;
        job.Paused = jobFbs.Paused;
        job.StartedAt = DateTimeOffset.FromUnixTimeMilliseconds(jobFbs.StartedAt).UtcDateTime;
        job.FinishedAt = DateTimeOffset.FromUnixTimeMilliseconds(jobFbs.FinishedAt).UtcDateTime;

        await appContext.SaveChangesAsync(cancellationToken);
        redis.Db.PublishAsync(RedisChannel.Literal($"jobs-active-{job.Device?.OwnerId}"), job.Id.ToString());

        return Result.Ok();
    }
}
