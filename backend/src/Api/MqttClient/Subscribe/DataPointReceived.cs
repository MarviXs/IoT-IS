using DataPointFlatBuffers;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using FluentResults;
using Google.FlatBuffers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.MqttClient.Subscribe;

public class DataPointReceived(AppDbContext appContext, RedisService redis, IHubContext<IsHub, INotificationsClient> hubContext)
{
    public async Task<Result> Handle(string deviceAccessToken, ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        var redisKey = $"device:{deviceAccessToken}:id";

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

        var datapoint = DataPointFbs.GetRootAsDataPointFbs(new ByteBuffer(payload.ToArray()));
        var redisResult = await redis.Db.StreamAddAsync(
            "datapoints",
            [
                new("device_id", deviceId.ToString()),
                new("sensor_tag", datapoint.Tag),
                new("value", datapoint.Value),
                new("timestamp", GetDataPointTimeStampOrCurrentTime(datapoint.Ts).ToUnixTimeMilliseconds())
            ],
            maxLength: 500000
        );

        redis.Db.StringSetAsync($"device:{deviceId}:connected", "1", TimeSpan.FromMinutes(30));
        redis.Db.StringSetAsync($"device:{deviceId}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        redis.Db.StringSetAsync($"device:{deviceId}:{datapoint.Tag}:last", datapoint.Value, TimeSpan.FromHours(1));
        await hubContext.Clients.Group(deviceId).ReceiveSensorLastDataPoint(new SensorLastDataPointDto(deviceId, datapoint.Tag.ToString(), datapoint.Value));

        return Result.Ok();
    }

    private static DateTimeOffset GetDataPointTimeStampOrCurrentTime(long? timeStamp)
    {
        return timeStamp.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(timeStamp.Value) : DateTimeOffset.UtcNow;
    }
}
