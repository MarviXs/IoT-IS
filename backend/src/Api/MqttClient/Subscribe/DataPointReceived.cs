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
    IHubContext<IsHub, INotificationsClient> hubContext,
    ILogger<DataPointReceived> logger
)
{
    private const string DataPointsStreamName = "datapoints";
    private const string SampleRateTag = "samplerate";

    public async Task<Result> Handle(string deviceAccessToken, ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        if (payload.Array == null)
        {
            return Result.Fail("Payload is missing");
        }

        var deviceGuid = await deviceAccessTokenResolver.ResolveDeviceIdAsync(deviceAccessToken, cancellationToken);
        if (!deviceGuid.HasValue)
        {
            return Result.Fail("Device not found");
        }
        var deviceIdGuid = deviceGuid.Value;
        var deviceId = deviceIdGuid.ToString();

        var datapoint = DataPointFbs.GetRootAsDataPointFbs(new ByteBuffer(payload.Array, payload.Offset));
        var timestamp = UnixTimestampUtils.NormalizeToDateTimeOffsetOrNow(datapoint.Ts);
        var sensorTag = datapoint.Tag;
        var value = datapoint.Value;

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return Result.Fail("Invalid datapoint value");
        }

        var updateSampleRateTask = UpdateSampleRateIfNeeded(deviceIdGuid, sensorTag, value, cancellationToken);

        var streamEntriesCount = 4;
        if (datapoint.Latitude.HasValue)
        {
            streamEntriesCount++;
        }
        if (datapoint.Longitude.HasValue)
        {
            streamEntriesCount++;
        }
        if (datapoint.GridX.HasValue)
        {
            streamEntriesCount++;
        }
        if (datapoint.GridY.HasValue)
        {
            streamEntriesCount++;
        }

        var streamEntries = new NameValueEntry[streamEntriesCount];
        var entryIndex = 0;
        streamEntries[entryIndex++] = new("device_id", deviceId);
        streamEntries[entryIndex++] = new("sensor_tag", sensorTag);
        streamEntries[entryIndex++] = new("value", value);
        streamEntries[entryIndex++] = new("timestamp", timestamp.ToUnixTimeMilliseconds());

        if (datapoint.Latitude.HasValue)
        {
            streamEntries[entryIndex++] = new("latitude", datapoint.Latitude.Value);
        }
        if (datapoint.Longitude.HasValue)
        {
            streamEntries[entryIndex++] = new("longitude", datapoint.Longitude.Value);
        }
        if (datapoint.GridX.HasValue)
        {
            streamEntries[entryIndex++] = new("grid_x", datapoint.GridX.Value);
        }
        if (datapoint.GridY.HasValue)
        {
            streamEntries[entryIndex++] = new("grid_y", datapoint.GridY.Value);
        }

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
        var streamAddTask = redis.Db.StreamAddAsync(DataPointsStreamName, streamEntries, maxLength: 500000);

        _ = redis.Db.StringSetAsync(
            $"device:{deviceId}:connected",
            "1",
            TimeSpan.FromMinutes(30),
            flags: CommandFlags.FireAndForget
        );
        _ = redis.Db.StringSetAsync(
            $"device:{deviceId}:lastSeen",
            nowUnixSeconds,
            flags: CommandFlags.FireAndForget
        );
        _ = redis.Db.StringSetAsync(
            $"device:{deviceId}:{sensorTag}:last",
            lastValueJson,
            TimeSpan.FromHours(1),
            flags: CommandFlags.FireAndForget
        );
        ObserveBackgroundTask(
            hubContext
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
            ),
            "SignalR notification failed for device {DeviceId} and sensor {SensorTag}",
            deviceId,
            sensorTag
        );
        await Task.WhenAll(streamAddTask, updateSampleRateTask);

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

    private void ObserveBackgroundTask(Task task, string message, string deviceId, string sensorTag)
    {
        if (task.IsCompletedSuccessfully)
        {
            return;
        }

        _ = task.ContinueWith(
            static (completedTask, state) =>
            {
                if (completedTask.IsCanceled)
                {
                    return;
                }

                var (log, logMessage, loggedDeviceId, loggedSensorTag) =
                    ((ILogger<DataPointReceived>, string, string, string))state!;

                log.LogWarning(completedTask.Exception, logMessage, loggedDeviceId, loggedSensorTag);
            },
            (logger, message, deviceId, sensorTag),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }
}
