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
                            .Scenes.Where(
                                s =>
                                    s.IsEnabled
                                    && s.SensorTriggers.Any(st => deviceIds.Contains(st.DeviceId) && sensorTags.Contains(st.SensorTag))
                            )
                            .Include(s => s.SensorTriggers)
                            .Include(s => s.Actions)
                            .AsSplitQuery()
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
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var redis = serviceScope.ServiceProvider.GetRequiredService<RedisService>();
        var timescale = serviceScope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();
        var pendingNotifications = new List<SceneNotification>();
        var nowUtc = DateTimeOffset.UtcNow;
        var evaluableScenes = scenes.Where(scene => CanEvaluateScene(scene, nowUtc)).ToList();
        var resolvedDataPoints = await ResolveMissingDataPointsAsync(
            incomingDataPoints,
            evaluableScenes,
            redis,
            timescale,
            cancellationToken
        );

        foreach (var scene in evaluableScenes)
        {
            var data = new JsonObject();
            var sensorValues = new Dictionary<(Guid DeviceId, string SensorTag), double?>();
            foreach (var trigger in scene.SensorTriggers)
            {
                var key = (trigger.DeviceId, trigger.SensorTag);

                if (!incomingDataPoints.TryGetValue(key, out var dataPoint))
                {
                    resolvedDataPoints.TryGetValue(key, out dataPoint);
                }

                sensorValues[key] = dataPoint?.Value;

                if (dataPoint != null)
                {
                    var devices = data["device"] as JsonObject ?? new JsonObject();
                    data["device"] = devices;

                    var deviceId = dataPoint.DeviceId.ToString();
                    var deviceSensors = devices[deviceId] as JsonObject ?? new JsonObject();
                    devices[deviceId] = deviceSensors;
                    deviceSensors[dataPoint.SensorTag] = dataPoint.Value;
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
                        await HandleTriggeredScene(scene, sensorValues, pendingNotifications, serviceScope, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error evaluating scene: {scene.Id}, Exception: {ex.Message}");
            }
        }

        if (dbContext.ChangeTracker.HasChanges())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (pendingNotifications.Count != 0)
        {
            await dbContext.BulkInsertAsync(pendingNotifications, cancellationToken: cancellationToken);
        }
    }

    private static bool CanEvaluateScene(Scene scene, DateTimeOffset nowUtc)
    {
        if (!scene.IsEnabled)
        {
            return false;
        }

        if (!scene.LastTriggeredAt.HasValue || scene.CooldownAfterTriggerTime <= 0)
        {
            return true;
        }

        return nowUtc >= scene.LastTriggeredAt.Value.AddSeconds(scene.CooldownAfterTriggerTime);
    }

    private static async Task<Dictionary<(Guid DeviceId, string SensorTag), DataPoint?>> ResolveMissingDataPointsAsync(
        IReadOnlyDictionary<(Guid DeviceId, string SensorTag), DataPoint> incomingDataPoints,
        IReadOnlyCollection<Scene> scenes,
        RedisService redis,
        TimeScaleDbContext timescale,
        CancellationToken cancellationToken
    )
    {
        var unresolvedKeys = scenes
            .SelectMany(scene => scene.SensorTriggers)
            .Select(trigger => (trigger.DeviceId, trigger.SensorTag))
            .Where(key => !incomingDataPoints.ContainsKey(key))
            .Distinct()
            .ToArray();

        var resolvedDataPoints = new Dictionary<(Guid DeviceId, string SensorTag), DataPoint?>(unresolvedKeys.Length);
        if (unresolvedKeys.Length == 0)
        {
            return resolvedDataPoints;
        }

        var redisKeys = unresolvedKeys.Select(static key => (RedisKey)GetLatestDataPointCacheKey(key.DeviceId, key.SensorTag)).ToArray();
        var cachedValues = await redis.Db.StringGetAsync(redisKeys);
        var dbFallbackKeys = new List<(Guid DeviceId, string SensorTag)>();

        for (var i = 0; i < unresolvedKeys.Length; i++)
        {
            var key = unresolvedKeys[i];
            var cachedDataPoint = TryCreateDataPointFromCacheValue(key.DeviceId, key.SensorTag, cachedValues[i]);
            if (cachedDataPoint != null)
            {
                resolvedDataPoints[key] = cachedDataPoint;
                continue;
            }

            dbFallbackKeys.Add(key);
        }

        foreach (var key in dbFallbackKeys)
        {
            resolvedDataPoints[key] = await GetDataPointFromDb(key.DeviceId, key.SensorTag, timescale, cancellationToken);
        }

        return resolvedDataPoints;
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
        List<SceneNotification> pendingNotifications,
        IServiceScope serviceScope,
        CancellationToken cancellationToken
    )
    {
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
                    Id = Guid.NewGuid(),
                    SceneId = scene.Id,
                    Message = message,
                    Severity = action.NotificationSeverity
                };
                pendingNotifications.Add(notification);
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
    }

    private static DataPoint? TryCreateDataPointFromCacheValue(Guid deviceId, string sensorTag, RedisValue cacheResult)
    {
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
            DeviceId = deviceId,
            SensorTag = sensorTag,
            TimeStamp = latestDataPoint.Ts ?? DateTimeOffset.UtcNow,
            Value = latestDataPoint.Value.Value,
            Latitude = latestDataPoint.Latitude,
            Longitude = latestDataPoint.Longitude,
            GridX = latestDataPoint.GridX,
            GridY = latestDataPoint.GridY
        };
    }

    private static string GetLatestDataPointCacheKey(Guid deviceId, string sensorTag) => $"device:{deviceId}:{sensorTag}:last";

    private static async Task<DataPoint?> GetDataPointFromDb(
        Guid deviceId,
        string sensorTag,
        TimeScaleDbContext timescale,
        CancellationToken cancellationToken
    )
    {
        return await LatestDataPointReader.GetLatestAsync(
            timescale,
            deviceId,
            sensorTag,
            null,
            null,
            cancellationToken
        );
    }
}
