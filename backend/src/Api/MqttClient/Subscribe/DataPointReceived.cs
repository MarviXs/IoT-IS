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
        var result = await HandleBatch([(deviceAccessToken, payload)], cancellationToken);
        return result.IsSuccess ? Result.Ok() : Result.Fail(result.Errors);
    }

    public async Task<Result> HandleBatch(
        IReadOnlyList<(string DeviceAccessToken, ArraySegment<byte> Payload)> messages,
        CancellationToken cancellationToken
    )
    {
        if (messages.Count == 0)
        {
            return Result.Ok();
        }

        var deviceIdsByToken = new Dictionary<string, Guid>(StringComparer.Ordinal);
        var parsedDataPoints = new List<ParsedDataPoint>(messages.Count);
        var sampleRatesToUpdate = new Dictionary<Guid, float>();

        foreach (var (deviceAccessToken, payload) in messages)
        {
            if (payload.Array == null)
            {
                logger.LogWarning("Skipping MQTT datapoint because payload is missing for access token {DeviceAccessToken}", deviceAccessToken);
                continue;
            }

            if (!deviceIdsByToken.TryGetValue(deviceAccessToken, out var deviceId))
            {
                var resolvedDeviceId = await deviceAccessTokenResolver.ResolveDeviceIdAsync(deviceAccessToken, cancellationToken);
                if (!resolvedDeviceId.HasValue)
                {
                    logger.LogWarning("Skipping MQTT datapoint because device was not found for access token {DeviceAccessToken}", deviceAccessToken);
                    continue;
                }

                deviceId = resolvedDeviceId.Value;
                deviceIdsByToken[deviceAccessToken] = deviceId;
            }

            var datapoint = DataPointFbs.GetRootAsDataPointFbs(new ByteBuffer(payload.Array, payload.Offset));
            var timestamp = UnixTimestampUtils.NormalizeToDateTimeOffsetOrNow(datapoint.Ts);
            var sensorTag = datapoint.Tag;
            var value = datapoint.Value;

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                logger.LogWarning(
                    "Skipping MQTT datapoint because value is invalid for device {DeviceId} and sensor {SensorTag}",
                    deviceId,
                    sensorTag
                );
                continue;
            }

            var deviceIdString = deviceId.ToString();
            parsedDataPoints.Add(
                new ParsedDataPoint(
                    deviceId,
                    deviceIdString,
                    sensorTag,
                    value,
                    timestamp,
                    datapoint.Latitude,
                    datapoint.Longitude,
                    datapoint.GridX,
                    datapoint.GridY
                )
            );

            if (string.Equals(sensorTag, SampleRateTag, StringComparison.OrdinalIgnoreCase) && value > 0)
            {
                sampleRatesToUpdate[deviceId] = (float)value;
            }
        }

        if (parsedDataPoints.Count == 0)
        {
            return Result.Ok();
        }

        var updateSampleRateTask = UpdateSampleRatesIfNeeded(sampleRatesToUpdate, cancellationToken);
        var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var redisBatch = redis.Db.CreateBatch();
        var streamAddTasks = new Task<RedisValue>[parsedDataPoints.Count];
        var notificationsByDevice = new Dictionary<string, List<SensorLastDataPointDto>>(StringComparer.Ordinal);

        for (var i = 0; i < parsedDataPoints.Count; i++)
        {
            var datapoint = parsedDataPoints[i];
            var streamEntries = CreateStreamEntries(datapoint);
            var latestResponse = new GetLatestDataPoints.Response(
                datapoint.Timestamp,
                datapoint.Value,
                datapoint.Latitude,
                datapoint.Longitude,
                datapoint.GridX,
                datapoint.GridY
            );

            streamAddTasks[i] = redisBatch.StreamAddAsync(DataPointsStreamName, streamEntries, maxLength: 500000);
            _ = redisBatch.StringSetAsync(
                $"device:{datapoint.DeviceIdString}:connected",
                "1",
                TimeSpan.FromMinutes(30),
                flags: CommandFlags.FireAndForget
            );
            _ = redisBatch.StringSetAsync(
                $"device:{datapoint.DeviceIdString}:lastSeen",
                nowUnixSeconds,
                flags: CommandFlags.FireAndForget
            );
            _ = redisBatch.StringSetAsync(
                $"device:{datapoint.DeviceIdString}:{datapoint.SensorTag}:last",
                JsonSerializer.Serialize(latestResponse),
                TimeSpan.FromHours(1),
                flags: CommandFlags.FireAndForget
            );

            if (!notificationsByDevice.TryGetValue(datapoint.DeviceIdString, out var notifications))
            {
                notifications = new List<SensorLastDataPointDto>();
                notificationsByDevice[datapoint.DeviceIdString] = notifications;
            }

            notifications.Add(
                new SensorLastDataPointDto(
                    datapoint.DeviceIdString,
                    datapoint.SensorTag,
                    datapoint.Value,
                    datapoint.Latitude,
                    datapoint.Longitude,
                    datapoint.GridX,
                    datapoint.GridY,
                    datapoint.Timestamp
                )
            );
        }

        redisBatch.Execute();
        await Task.WhenAll(streamAddTasks.Cast<Task>().Append(updateSampleRateTask));
        foreach (var (deviceId, notifications) in notificationsByDevice)
        {
            ObserveBackgroundTask(
                hubContext.Clients.Group(deviceId).ReceiveSensorLastDataPoints(notifications),
                "SignalR notification batch failed for device {DeviceId}",
                deviceId
            );
        }

        return Result.Ok();
    }

    private static NameValueEntry[] CreateStreamEntries(ParsedDataPoint datapoint)
    {
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
        streamEntries[entryIndex++] = new("device_id", datapoint.DeviceIdString);
        streamEntries[entryIndex++] = new("sensor_tag", datapoint.SensorTag);
        streamEntries[entryIndex++] = new("value", datapoint.Value);
        streamEntries[entryIndex++] = new("timestamp", datapoint.Timestamp.ToUnixTimeMilliseconds());

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

        return streamEntries;
    }

    private async Task UpdateSampleRatesIfNeeded(IReadOnlyDictionary<Guid, float> sampleRatesToUpdate, CancellationToken cancellationToken)
    {
        if (sampleRatesToUpdate.Count == 0)
        {
            return;
        }

        var deviceIds = sampleRatesToUpdate.Keys.ToArray();
        var devices = await appContext.Devices.Where(device => deviceIds.Contains(device.Id)).ToListAsync(cancellationToken);
        var hasChanges = false;

        foreach (var device in devices)
        {
            if (!sampleRatesToUpdate.TryGetValue(device.Id, out var sampleRate))
            {
                continue;
            }

            if (device.SampleRateSeconds == sampleRate)
            {
                continue;
            }

            device.SampleRateSeconds = sampleRate;
            hasChanges = true;
        }

        if (hasChanges)
        {
            await appContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void ObserveBackgroundTask(Task task, string message, string deviceId)
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

                var (log, logMessage, loggedDeviceId) = ((ILogger<DataPointReceived>, string, string))state!;

                log.LogWarning(completedTask.Exception, logMessage, loggedDeviceId);
            },
            (logger, message, deviceId),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    private readonly record struct ParsedDataPoint(
        Guid DeviceId,
        string DeviceIdString,
        string SensorTag,
        double Value,
        DateTimeOffset Timestamp,
        double? Latitude,
        double? Longitude,
        int? GridX,
        int? GridY
    );
}
