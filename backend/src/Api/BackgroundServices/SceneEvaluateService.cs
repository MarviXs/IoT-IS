using System.Globalization;
using System.Text.Json.Nodes;
using EFCore.BulkExtensions;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Jobs.Services;
using Fei.Is.Api.Redis;
using Json.Logic;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.BackgroundServices;

public class SceneEvaluateService(IServiceProvider serviceProvider, ILogger<SceneEvaluateService> logger) : BackgroundService
{
    private const string StreamName = "datapoints";
    private const string GroupName = "evaluate_scene";
    private const int ProcessingSpeed = 50;

    static Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

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

                // Create consumer group if it doesn't exist
                if (!await redis.Db.KeyExistsAsync(StreamName) || (await redis.Db.StreamGroupInfoAsync(StreamName)).All(x => x.Name != GroupName))
                {
                    await redis.Db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0", true);
                }

                var messages = await redis.Db.StreamReadGroupAsync(StreamName, GroupName, consumerName, ">");

                if (messages.Length != 0)
                {
                    var dataPoints = new List<DataPoint>();

                    foreach (var message in messages)
                    {
                        try
                        {
                            var parsed = ParseResult(message);

                            if (
                                !double.TryParse(
                                    parsed["value"],
                                    NumberStyles.Float,
                                    CultureInfo.InvariantCulture,
                                    out var value
                                )
                            )
                            {
                                continue;
                            }

                            double? latitude = null;
                            if (
                                parsed.TryGetValue("latitude", out var latitudeRaw)
                                && double.TryParse(latitudeRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitudeValue)
                            )
                            {
                                latitude = latitudeValue;
                            }

                            double? longitude = null;
                            if (
                                parsed.TryGetValue("longitude", out var longitudeRaw)
                                && double.TryParse(longitudeRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitudeValue)
                            )
                            {
                                longitude = longitudeValue;
                            }

                            int? gridX = null;
                            if (
                                parsed.TryGetValue("grid_x", out var gridXRaw)
                                && int.TryParse(gridXRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridXValue)
                            )
                            {
                                gridX = gridXValue;
                            }

                            int? gridY = null;
                            if (
                                parsed.TryGetValue("grid_y", out var gridYRaw)
                                && int.TryParse(gridYRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridYValue)
                            )
                            {
                                gridY = gridYValue;
                            }

                            dataPoints.Add(
                                new DataPoint
                                {
                                    DeviceId = Guid.Parse(parsed["device_id"]),
                                    SensorTag = parsed["sensor_tag"],
                                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(parsed["timestamp"])),
                                    Value = value,
                                    Latitude = latitude,
                                    Longitude = longitude,
                                    GridX = gridX,
                                    GridY = gridY
                                }
                            );
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error parsing datapoints message: {MessageId}", message.Id);
                        }
                    }

                    if (dataPoints.Count != 0)
                    {
                        var dataPointKeys = dataPoints.Select(dp => $"{dp.SensorTag}{dp.DeviceId}").ToHashSet();

                        var scenes = await dbContext
                            .Scenes.Where(s => s.SensorTriggers.Any(st => dataPointKeys.Contains(st.SensorTag + st.DeviceId)))
                            .Include(s => s.SensorTriggers)
                            .ToListAsync(cancellationToken);

                        await EvaluateScenes(dataPoints, scenes, scope);

                        var messageIds = messages.Select(m => m.Id).ToArray();
                        await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, messageIds);
                    }
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

    private async Task EvaluateScenes(List<DataPoint> dataPoints, List<Scene> scenes, IServiceScope serviceScope)
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var redis = serviceScope.ServiceProvider.GetRequiredService<RedisService>();
        var timescale = serviceScope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();

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
                if (DateTimeOffset.UtcNow < cooldownEnd)
                {
                    continue;
                }
            }

            var data = new JsonObject();
            foreach (var trigger in scene.SensorTriggers)
            {
                var dataPoint = dataPoints.FirstOrDefault(dp => dp.DeviceId == trigger.DeviceId && dp.SensorTag == trigger.SensorTag);
                dataPoint ??= await GetDataPointFromCache(trigger, redis);
                dataPoint ??= await GetDataPointFromDb(trigger, timescale);

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
                        await HandleTriggeredScene(scene, serviceScope);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error evaluating scene: {scene.Id}, Exception: {ex.Message}");
            }
        }
    }

    private async Task HandleTriggeredScene(Scene scene, IServiceScope serviceScope)
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobService = serviceScope.ServiceProvider.GetRequiredService<JobService>();

        scene.LastTriggeredAt = DateTimeOffset.UtcNow;

        var actions = scene.Actions;

        foreach (var action in actions)
        {
            if (action.Type == SceneActionType.NOTIFICATION)
            {
                var notification = new SceneNotification
                {
                    SceneId = scene.Id,
                    Message = action.NotificationMessage ?? "Scene triggered",
                    Severity = action.NotificationSeverity
                };
                dbContext.SceneNotifications.Add(notification);
            }
            else if (action.Type == SceneActionType.JOB && action.RecipeId.HasValue && action.DeviceId.HasValue)
            {
                await jobService.CreateJobFromRecipe(action.DeviceId.Value, action.RecipeId.Value, 1, false, CancellationToken.None);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<DataPoint?> GetDataPointFromCache(SceneSensorTrigger trigger, RedisService redis)
    {
        var cacheResult = await redis.Db.StringGetAsync($"device:{trigger.DeviceId}:{trigger.SensorTag}:last");
        return cacheResult.HasValue
            ? new DataPoint
            {
                DeviceId = trigger.DeviceId,
                SensorTag = trigger.SensorTag,
                Value = (double)cacheResult,
            }
            : null;
    }

    private static async Task<DataPoint?> GetDataPointFromDb(SceneSensorTrigger trigger, TimeScaleDbContext timescale)
    {
        return await timescale
            .DataPoints.Where(dp => dp.DeviceId == trigger.DeviceId && dp.SensorTag == trigger.SensorTag)
            .OrderByDescending(dp => dp.TimeStamp)
            .FirstOrDefaultAsync();
    }
}
