using System.Text.Json;
using DataPointFlatBuffers;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using FluentResults;
using Google.FlatBuffers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.MqttClient.Subscribe;

public class DataPointReceived(AppDbContext appContext, RedisService redis, IHubContext<IsHub, INotificationsClient> hubContext)
{
    public async Task<Result> Handle(string deviceAccessToken, ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        var redisKey = $"device:{deviceAccessToken}:id";

        // Check if deviceId is in Redis cache
        var cachedDeviceId = await redis.Db.StringGetAsync(redisKey);

        string deviceId;
        Guid deviceGuid;

        if (cachedDeviceId.HasValue)
        {
            deviceId = cachedDeviceId!;
            if (!Guid.TryParse(deviceId, out deviceGuid))
            {
                return Result.Fail("Invalid device ID");
            }
        }
        else
        {
            deviceGuid = await appContext
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
        var timestamp = GetDataPointTimeStampOrCurrentTime(datapoint.Ts);

        if (double.IsNaN(datapoint.Value) || double.IsInfinity(datapoint.Value))
        {
            return Result.Fail("Invalid datapoint value");
        }

        await UpdateSampleRateIfNeeded(deviceGuid, datapoint.Tag, datapoint.Value, cancellationToken);

        var redisResult = await redis.Db.StreamAddAsync(
            "datapoints",
            [
                new("device_id", deviceId.ToString()),
                new("sensor_tag", datapoint.Tag),
                new("value", datapoint.Value),
                new("timestamp", timestamp.ToUnixTimeMilliseconds()),
                new("latitude", datapoint.Latitude),
                new("longitude", datapoint.Longitude),
                new("grid_x", datapoint.GridX),
                new("grid_y", datapoint.GridY)
            ],
            maxLength: 500000
        );

        

        await redis.Db.StringSetAsync($"device:{deviceId}:connected", "1", TimeSpan.FromMinutes(30), flags: CommandFlags.FireAndForget);
        await redis.Db.StringSetAsync($"device:{deviceId}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), flags: CommandFlags.FireAndForget);

        var latestResponse = new GetLatestDataPoints.Response(
            timestamp,
            datapoint.Value,
            datapoint.Latitude,
            datapoint.Longitude,
            datapoint.GridX,
            datapoint.GridY
        );

        await redis.Db.StringSetAsync(
            $"device:{deviceId}:{datapoint.Tag}:last",
            JsonSerializer.Serialize(latestResponse),
            TimeSpan.FromHours(1),
            flags: CommandFlags.FireAndForget
        );

        await hubContext
            .Clients.Group(deviceId)
            .ReceiveSensorLastDataPoint(
                new SensorLastDataPointDto(
                    deviceId,
                    datapoint.Tag.ToString(),
                    datapoint.Value,
                    datapoint.Latitude,
                    datapoint.Longitude,
                    datapoint.GridX,
                    datapoint.GridY,
                    timestamp
                )
            );

        return Result.Ok();
    }

    private async Task UpdateSampleRateIfNeeded(Guid deviceId, string? tag, double value, CancellationToken cancellationToken)
    {
        if (!string.Equals(tag, "samplerate", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            return;
        }

        var device = await appContext.Devices.FindAsync([deviceId], cancellationToken);
        if (device == null)
        {
            return;
        }

        device.SampleRateSeconds = (float)value;
        await appContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset GetDataPointTimeStampOrCurrentTime(long? timeStamp)
    {
        if (!timeStamp.HasValue)
            return DateTimeOffset.UtcNow;

        var date = DateTimeOffset.FromUnixTimeMilliseconds(timeStamp.Value);
        return date.Year < 2000 ? DateTimeOffset.UtcNow : date;
    }
}
