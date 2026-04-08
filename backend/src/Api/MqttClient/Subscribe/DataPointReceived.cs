using System.Text.Json;
using DataPointFlatBuffers;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Features.Devices.Services;
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

public class DataPointReceived(
    AppDbContext appContext,
    DeviceAccessTokenResolver deviceAccessTokenResolver,
    RedisService redis,
    IHubContext<IsHub, INotificationsClient> hubContext
)
{
    private const string DataPointsStreamName = "datapoints";
    private const string SampleRateTag = "samplerate";

    public async Task<Result> Handle(string deviceAccessToken, ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        var deviceGuid = await deviceAccessTokenResolver.ResolveDeviceIdAsync(deviceAccessToken, cancellationToken);
        if (!deviceGuid.HasValue)
        {
            return Result.Fail("Device not found");
        }
        var deviceIdGuid = deviceGuid.Value;
        var deviceId = deviceIdGuid.ToString();

        var datapoint = DataPointFbs.GetRootAsDataPointFbs(new ByteBuffer(payload.ToArray()));
        var timestamp = UnixTimestampUtils.NormalizeToDateTimeOffsetOrNow(datapoint.Ts);
        var sensorTag = datapoint.Tag;
        var value = datapoint.Value;

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return Result.Fail("Invalid datapoint value");
        }

        var updateSampleRateTask = UpdateSampleRateIfNeeded(deviceIdGuid, sensorTag, value, cancellationToken);

        var streamEntries = new List<NameValueEntry>(8)
        {
            new("device_id", deviceId),
            new("sensor_tag", sensorTag),
            new("value", value),
            new("timestamp", timestamp.ToUnixTimeMilliseconds())
        };

        if (datapoint.Latitude.HasValue)
        {
            streamEntries.Add(new("latitude", datapoint.Latitude.Value));
        }
        if (datapoint.Longitude.HasValue)
        {
            streamEntries.Add(new("longitude", datapoint.Longitude.Value));
        }
        if (datapoint.GridX.HasValue)
        {
            streamEntries.Add(new("grid_x", datapoint.GridX.Value));
        }
        if (datapoint.GridY.HasValue)
        {
            streamEntries.Add(new("grid_y", datapoint.GridY.Value));
        }

        var streamAddTask = redis.Db.StreamAddAsync(DataPointsStreamName, streamEntries.ToArray(), maxLength: 500000);

        var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var latestResponse = new GetLatestDataPoints.Response(
            timestamp,
            value,
            datapoint.Latitude,
            datapoint.Longitude,
            datapoint.GridX,
            datapoint.GridY
        );

        var lastValueJson = JsonSerializer.Serialize(latestResponse);
        var connectedTask = redis.Db.StringSetAsync($"device:{deviceId}:connected", "1", TimeSpan.FromMinutes(30), flags: CommandFlags.FireAndForget);
        var lastSeenTask = redis.Db.StringSetAsync($"device:{deviceId}:lastSeen", nowUnixSeconds, flags: CommandFlags.FireAndForget);
        var lastValueTask = redis.Db.StringSetAsync(
            $"device:{deviceId}:{sensorTag}:last",
            lastValueJson,
            TimeSpan.FromHours(1),
            flags: CommandFlags.FireAndForget
        );
        var notifyTask = hubContext
            .Clients.Group(deviceId)
            .ReceiveSensorLastDataPoint(
                new SensorLastDataPointDto(
                    deviceId,
                    sensorTag,
                    value,
                    datapoint.Latitude,
                    datapoint.Longitude,
                    datapoint.GridX,
                    datapoint.GridY,
                    timestamp
                )
            );
        await Task.WhenAll(streamAddTask, updateSampleRateTask, connectedTask, lastSeenTask, lastValueTask, notifyTask);

        return Result.Ok();
    }

    private Task UpdateSampleRateIfNeeded(Guid deviceId, string? tag, double value, CancellationToken cancellationToken)
    {
        if (!string.Equals(tag, SampleRateTag, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }
        if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
        {
            return Task.CompletedTask;
        }

        return UpdateSampleRateAsync(deviceId, value, cancellationToken);
    }

    private async Task UpdateSampleRateAsync(Guid deviceId, double value, CancellationToken cancellationToken)
    {
        var device = await appContext.Devices.FindAsync([deviceId], cancellationToken);
        if (device == null)
        {
            return;
        }

        device.SampleRateSeconds = (float)value;
        await appContext.SaveChangesAsync(cancellationToken);
    }
}
