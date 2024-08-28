using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.MqttClient.Proto;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProtoBuf;

namespace Fei.Is.Api.MqttClient.Commands;

public static class JobStatusReceived
{
    public record Command(string DeviceAccessToken, ArraySegment<byte> Payload) : IRequest<Result>;

    public sealed class Handler(AppDbContext appContext, RedisService redis) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var redisKey = $"device:{message.DeviceAccessToken}:id";

            // Check if deviceId is in Redis cache
            var cachedDeviceId = await redis.Db.StringGetAsync(redisKey);

            string deviceId;

            if (cachedDeviceId.HasValue)
            {
                deviceId = cachedDeviceId!;
            }
            else
            {
                var deviceGuid = await appContext
                    .Devices.Where(d => d.AccessToken == message.DeviceAccessToken)
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

            var jobProto = Serializer.Deserialize<JobProto>(message.Payload.AsMemory());

            var job = await appContext
                .Jobs.Include(j => j.Commands.OrderBy(c => c.Order))
                .Where(j => j.DeviceId == Guid.Parse(deviceId))
                .FirstOrDefaultAsync(cancellationToken);
                
            if (job == null)
            {
                return Result.Fail(new NotFoundError());
            }

            job.Status = jobProto.Status;
            job.CurrentStep = jobProto.CurrentStep;
            job.TotalSteps = jobProto.TotalSteps;
            job.CurrentCycle = jobProto.CurrentCycle;
            job.TotalCycles = jobProto.TotalCycles;
            job.Paused = jobProto.Paused;
            job.StartedAt = jobProto.StartedAt;
            job.FinishedAt = jobProto.FinishedAt;

            await appContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
