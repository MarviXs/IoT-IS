using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using EFCore.BulkExtensions;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Features.DataPoints.Services;
using Fei.Is.Api.Features.Jobs.Services;
using Fei.Is.Api.Features.Scenes.Services;
using Fei.Is.Api.Redis;
using Fei.Is.Api.Services.Notifications;
using Json.Logic;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.BackgroundServices;

public class SceneEvaluateService(IServiceProvider serviceProvider, ILogger<SceneEvaluateService> logger) : BackgroundService
{
    private const string StreamName = "datapoints";
    private const string GroupName = "evaluate_scene";
    private const int ProcessingSpeed = 50;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string consumerName = Guid.NewGuid().ToString();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var timescale = scope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var redis = scope.ServiceProvider.GetRequiredService<RedisService>();
                var sceneDeviceCache = scope.ServiceProvider.GetRequiredService<SceneDeviceCacheService>();

                // Create consumer group if it doesn't exist
                if (!await redis.Db.KeyExistsAsync(StreamName) || (await redis.Db.StreamGroupInfoAsync(StreamName)).All(x => x.Name != GroupName))
                {
                    await redis.Db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0", true);
                }

                var messages = await redis.Db.StreamReadGroupAsync(StreamName, GroupName, consumerName, ">");

                if (messages.Length != 0)
                {
                    var dataPoints = new List<DataPoint>(messages.Length);

                    foreach (var message in messages)
                    {
                        try
                        {
                            if (!TryParseDataPoint(message, out var dataPoint))
                            {
                                continue;
                            }

                            dataPoints.Add(dataPoint);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error parsing datapoints message: {MessageId}", message.Id);
                        }
                    }

                    var messageIds = messages.Select(m => m.Id).ToArray();

                    if (dataPoints.Count != 0)
                    {
                        var dataPointLookup = new Dictionary<(Guid DeviceId, string SensorTag), DataPoint>(dataPoints.Count);
                        var deviceIds = new HashSet<Guid>();
                        var sensorTags = new HashSet<string>(StringComparer.Ordinal);

                        foreach (var dataPoint in dataPoints)
                        {
                            dataPointLookup[(dataPoint.DeviceId, dataPoint.SensorTag)] = dataPoint;
                            deviceIds.Add(dataPoint.DeviceId);
                            sensorTags.Add(dataPoint.SensorTag);
                        }

                        if (!await sceneDeviceCache.AnyEnabledScenesForDevicesAsync(dbContext, deviceIds, cancellationToken))
                        {
                            await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, messageIds);
                            await Task.Delay(ProcessingSpeed, cancellationToken);
                            continue;
                        }

                        var scenes = await dbContext
                            .Scenes.Where(s => s.SensorTriggers.Any(st => deviceIds.Contains(st.DeviceId) && sensorTags.Contains(st.SensorTag)))
                            .Include(s => s.SensorTriggers)
                            .Include(s => s.Actions)
                            .ToListAsync(cancellationToken);

                        scenes = scenes
                            .Where(s => s.SensorTriggers.Any(st => dataPointLookup.ContainsKey((st.DeviceId, st.SensorTag))))
                            .ToList();

                        if (scenes.Count != 0)
                        {
                            await EvaluateScenes(dataPointLookup, scenes, scope, cancellationToken);
                        }
                    }

                    await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, messageIds);
                }
                await Task.Delay(ProcessingSpeed, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing datapoints messages: {ExceptionMessage}", ex.Message);
                await Task.Delay(ProcessingSpeed, cancellationToken);
            }
        }
    }

    private async Task EvaluateScenes(
        IReadOnlyDictionary<(Guid DeviceId, string SensorTag), DataPoint> incomingDataPoints,
        List<Scene> scenes,
        IServiceScope serviceScope,
        CancellationToken cancellationToken
    )
    {
        var redis = serviceScope.ServiceProvider.GetRequiredService<RedisService>();
        var timescale = serviceScope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();
        var resolvedDataPoints = new Dictionary<(Guid DeviceId, string SensorTag), DataPoint?>();
        var nowUtc = DateTimeOffset.UtcNow;

        foreach (var scene in scenes)
        {
            // Check if the scene is enabled
            if (!scene.IsEnabled)
            {
                continue;
            }

            // Check if the scene has a cooldown period
            if (scene.LastTriggeredAt.HasValue && scene.CooldownAfterTriggerTime > 0)
            {
                var cooldownEnd = scene.LastTriggeredAt.Value.AddSeconds(scene.CooldownAfterTriggerTime);
                if (nowUtc < cooldownEnd)
                {
                    continue;
                }
            }

            var data = new JsonObject();
            var sensorValues = new Dictionary<(Guid DeviceId, string SensorTag), double?>();
            foreach (var trigger in scene.SensorTriggers)
            {
                var key = (trigger.DeviceId, trigger.SensorTag);

                if (!incomingDataPoints.TryGetValue(key, out var dataPoint) && !resolvedDataPoints.TryGetValue(key, out dataPoint))
                {
                    dataPoint = await GetDataPointFromCache(trigger, redis);
                    dataPoint ??= await GetDataPointFromDb(trigger, timescale, cancellationToken);
                    resolvedDataPoints[key] = dataPoint;
                }

                sensorValues[key] = dataPoint?.Value;

                if (dataPoint != null)
                {
                    data["device"] ??= new JsonObject();
                    data["device"][dataPoint.DeviceId.ToString()] ??= new JsonObject();
                    data["device"][dataPoint.DeviceId.ToString()][dataPoint.SensorTag.ToString()] = dataPoint.Value;
                }
            }
            try
            {
                if (!string.IsNullOrEmpty(scene.Condition))
                {
                    var rule = JsonNode.Parse(scene.Condition);
                    var result = JsonLogic.Apply(rule, data);

                    if (result is JsonValue booleanResult && booleanResult.TryGetValue<bool>(out var isTriggered) && isTriggered)
                    {
                        await HandleTriggeredScene(scene, sensorValues, serviceScope, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error evaluating scene: {scene.Id}, Exception: {ex.Message}");
            }
        }
    }

    private static bool TryParseDataPoint(StreamEntry entry, out DataPoint dataPoint)
    {
        dataPoint = null!;

        string? sensorTag = null;
        Guid deviceId = Guid.Empty;
        double value = 0;
        bool hasDeviceId = false;
        bool hasValue = false;
        var timestamp = DateTimeOffset.UtcNow;
        double? latitude = null;
        double? longitude = null;
        int? gridX = null;
        int? gridY = null;

        foreach (var valueEntry in entry.Values)
        {
            var name = valueEntry.Name.ToString();
            var rawValue = valueEntry.Value.ToString();

            switch (name)
            {
                case "device_id":
                    hasDeviceId = Guid.TryParse(rawValue, out deviceId);
                    break;
                case "sensor_tag":
                    sensorTag = rawValue;
                    break;
                case "value":
                    hasValue = double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                        && !double.IsNaN(value)
                        && !double.IsInfinity(value);
                    break;
                case "timestamp":
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedTimestamp))
                    {
                        timestamp = UnixTimestampUtils.NormalizeToDateTimeOffsetOrNow(parsedTimestamp);
                    }
                    break;
                case "latitude":
                    if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitudeValue))
                    {
                        latitude = latitudeValue;
                    }
                    break;
                case "longitude":
                    if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitudeValue))
                    {
                        longitude = longitudeValue;
                    }
                    break;
                case "grid_x":
                    if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridXValue))
                    {
                        gridX = gridXValue;
                    }
                    break;
                case "grid_y":
                    if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridYValue))
                    {
                        gridY = gridYValue;
                    }
                    break;
            }
        }

        if (!hasDeviceId || !hasValue || string.IsNullOrEmpty(sensorTag))
        {
            return false;
        }

        dataPoint = new DataPoint
        {
            DeviceId = deviceId,
            SensorTag = sensorTag,
            TimeStamp = timestamp,
            Value = value,
            Latitude = latitude,
            Longitude = longitude,
            GridX = gridX,
            GridY = gridY
        };
        return true;
    }

    private async Task HandleTriggeredScene(
        Scene scene,
        Dictionary<(Guid DeviceId, string SensorTag), double?> sensorValues,
        IServiceScope serviceScope,
        CancellationToken cancellationToken
    )
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobService = serviceScope.ServiceProvider.GetRequiredService<JobService>();
        var discordNotificationService = serviceScope.ServiceProvider.GetRequiredService<IDiscordNotificationService>();

        scene.LastTriggeredAt = DateTimeOffset.UtcNow;

        var actions = scene.Actions;

        foreach (var action in actions)
        {
            if (action.Type == SceneActionType.NOTIFICATION)
            {
                var message = string.IsNullOrWhiteSpace(action.NotificationMessage) ? "Scene triggered" : action.NotificationMessage!;
                var notification = new SceneNotification
                {
                    SceneId = scene.Id,
                    Message = message,
                    Severity = action.NotificationSeverity
                };
                dbContext.SceneNotifications.Add(notification);
            }
            else if (action.Type == SceneActionType.DISCORD_NOTIFICATION)
            {
                var baseMessage = string.IsNullOrWhiteSpace(action.NotificationMessage) ? "Scene triggered" : action.NotificationMessage!;
                var sensorFields = new List<(string Name, string Value)>();

                if (action.IncludeSensorValues && scene.SensorTriggers.Count != 0)
                {
                    var seenSensors = new HashSet<(Guid DeviceId, string SensorTag)>();

                    foreach (var trigger in scene.SensorTriggers)
                    {
                        var key = (trigger.DeviceId, trigger.SensorTag);
                        if (!seenSensors.Add(key))
                        {
                            continue;
                        }

                        sensorValues.TryGetValue(key, out var value);
                        var formattedValue = value.HasValue ? value.Value.ToString("0.##", CultureInfo.InvariantCulture) : "N/A";
                        sensorFields.Add((trigger.SensorTag, formattedValue));
                    }
                }

                var fields = sensorFields.Count == 0 ? Array.Empty<(string Name, string Value)>() : [.. sensorFields];
                if (!string.IsNullOrWhiteSpace(action.DiscordWebhookUrl))
                {
                    await discordNotificationService.SendAsync(
                        action.DiscordWebhookUrl!,
                        scene.Name,
                        baseMessage,
                        fields,
                        action.NotificationSeverity,
                        cancellationToken
                    );
                }
                else
                {
                    logger.LogWarning("Scene {SceneId} Discord notification is missing a webhook URL.", scene.Id);
                }
            }
            else if (action.Type == SceneActionType.JOB && action.RecipeId.HasValue && action.DeviceId.HasValue)
            {
                await jobService.CreateJobFromRecipe(action.DeviceId.Value, action.RecipeId.Value, 1, false, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<DataPoint?> GetDataPointFromCache(SceneSensorTrigger trigger, RedisService redis)
    {
        var cacheResult = await redis.Db.StringGetAsync($"device:{trigger.DeviceId}:{trigger.SensorTag}:last");
        if (!cacheResult.HasValue)
        {
            return null;
        }

        var latestDataPoint = JsonSerializer.Deserialize<GetLatestDataPoints.Response>(cacheResult!);
        if (latestDataPoint?.Value == null)
        {
            return null;
        }

        return new DataPoint
        {
            DeviceId = trigger.DeviceId,
            SensorTag = trigger.SensorTag,
            TimeStamp = latestDataPoint.Ts ?? DateTimeOffset.UtcNow,
            Value = latestDataPoint.Value.Value,
            Latitude = latestDataPoint.Latitude,
            Longitude = latestDataPoint.Longitude,
            GridX = latestDataPoint.GridX,
            GridY = latestDataPoint.GridY
        };
    }

    private static async Task<DataPoint?> GetDataPointFromDb(
        SceneSensorTrigger trigger,
        TimeScaleDbContext timescale,
        CancellationToken cancellationToken
    )
    {
        return await LatestDataPointReader.GetLatestAsync(
            timescale,
            trigger.DeviceId,
            trigger.SensorTag,
            null,
            null,
            cancellationToken
        );
    }
}
