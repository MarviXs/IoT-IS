using System.Globalization;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.System.Services;
using Fei.Is.Api.Grpc;
using Fei.Is.Api.Redis;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.BackgroundServices;

public class EdgeHubDataSyncBackgroundService(IServiceProvider serviceProvider, ILogger<EdgeHubDataSyncBackgroundService> logger) : BackgroundService
{
    private const string StreamName = "datapoints";
    private const string GroupName = "edge_hub_sync";
    private const int MaxPendingTimeUnclaimed = 20000;
    private const int DefaultSyncSeconds = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerName = $"edge-{Guid.NewGuid()}";

        while (!stoppingToken.IsCancellationRequested)
        {
            var delaySeconds = DefaultSyncSeconds;
            try
            {
                using var scope = serviceProvider.CreateScope();
                var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var redis = scope.ServiceProvider.GetRequiredService<RedisService>();
                var grpcClientFactory = scope.ServiceProvider.GetRequiredService<HubGrpcClientFactory>();

                var settings = await appContext.SystemNodeSettings.OrderBy(setting => setting.CreatedAt).FirstOrDefaultAsync(stoppingToken);
                if (
                    settings == null
                    || settings.NodeType != SystemNodeType.Edge
                    || string.IsNullOrWhiteSpace(settings.HubUrl)
                    || string.IsNullOrWhiteSpace(settings.HubToken)
                )
                {
                    await Task.Delay(TimeSpan.FromSeconds(DefaultSyncSeconds), stoppingToken);
                    continue;
                }

                if (
                    !await redis.Db.KeyExistsAsync(StreamName)
                    || (await redis.Db.StreamGroupInfoAsync(StreamName)).All(group => group.Name != GroupName)
                )
                {
                    await redis.Db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "$", true);
                }

                var autoClaimResult = await redis.Db.StreamAutoClaimAsync(StreamName, GroupName, consumerName, MaxPendingTimeUnclaimed, "0-0", 5000);
                var messages = await redis.Db.StreamReadGroupAsync(StreamName, GroupName, consumerName, ">");
                messages = [.. messages, .. autoClaimResult.ClaimedEntries];

                var request = new SyncDataPointsRequest();
                var ackIds = new List<RedisValue>();

                if (messages.Length > 0)
                {
                    var candidateDeviceIds = new HashSet<Guid>();
                    foreach (var message in messages)
                    {
                        var parsed = ParseResult(message);
                        if (parsed.TryGetValue("device_id", out var rawDeviceId) && Guid.TryParse(rawDeviceId, out var deviceId))
                        {
                            candidateDeviceIds.Add(deviceId);
                        }
                    }

                    var syncedDeviceIds = await appContext
                        .Devices.AsNoTracking()
                        .Where(device => device.IsSyncedFromHub && candidateDeviceIds.Contains(device.Id))
                        .Select(device => device.Id)
                        .ToListAsync(stoppingToken);
                    var syncedSet = syncedDeviceIds.ToHashSet();

                    foreach (var message in messages)
                    {
                        try
                        {
                            var parsed = ParseResult(message);
                            ackIds.Add(message.Id);

                            if (
                                !parsed.TryGetValue("device_id", out var rawDeviceId)
                                || !Guid.TryParse(rawDeviceId, out var deviceId)
                                || !syncedSet.Contains(deviceId)
                            )
                            {
                                continue;
                            }

                            if (!parsed.TryGetValue("sensor_tag", out var sensorTag) || string.IsNullOrWhiteSpace(sensorTag))
                            {
                                continue;
                            }

                            if (
                                !parsed.TryGetValue("value", out var rawValue)
                                || !double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                                || double.IsNaN(value)
                                || double.IsInfinity(value)
                            )
                            {
                                continue;
                            }

                            if (
                                !parsed.TryGetValue("timestamp", out var rawTimestamp)
                                || !long.TryParse(rawTimestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestampUnixMs)
                            )
                            {
                                timestampUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            }

                            var dataPoint = new HubDataPoint
                            {
                                DeviceId = deviceId.ToString(),
                                SensorTag = sensorTag,
                                Value = value,
                                TimestampUnixMs = timestampUnixMs
                            };

                            if (
                                parsed.TryGetValue("latitude", out var rawLatitude)
                                && double.TryParse(rawLatitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
                            )
                            {
                                dataPoint.Latitude = latitude;
                            }
                            if (
                                parsed.TryGetValue("longitude", out var rawLongitude)
                                && double.TryParse(rawLongitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
                            )
                            {
                                dataPoint.Longitude = longitude;
                            }
                            if (
                                parsed.TryGetValue("grid_x", out var rawGridX)
                                && int.TryParse(rawGridX, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridX)
                            )
                            {
                                dataPoint.GridX = gridX;
                            }
                            if (
                                parsed.TryGetValue("grid_y", out var rawGridY)
                                && int.TryParse(rawGridY, NumberStyles.Integer, CultureInfo.InvariantCulture, out var gridY)
                            )
                            {
                                dataPoint.GridY = gridY;
                            }

                            request.Datapoints.Add(dataPoint);
                        }
                        catch (Exception parseException)
                        {
                            logger.LogWarning(parseException, "Failed to parse datapoint stream message for edge hub sync: {MessageId}", message.Id);
                        }
                    }
                }

                var (channel, client) = grpcClientFactory.Create(settings.HubUrl!);
                try
                {
                    var headers = new Metadata { { "x-edge-token", settings.HubToken!.Trim() } };
                    var response = await client.SyncDataPointsAsync(request, headers, cancellationToken: stoppingToken);
                    delaySeconds = response.NextSyncSeconds > 0 ? response.NextSyncSeconds : DefaultSyncSeconds;

                    await redis.Db.StringSetAsync(
                        "edge:hub:last-sync",
                        DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                        TimeSpan.FromDays(7),
                        flags: CommandFlags.FireAndForget
                    );
                    await redis.Db.StringSetAsync(
                        "edge:hub:expected-sync-seconds",
                        delaySeconds.ToString(CultureInfo.InvariantCulture),
                        TimeSpan.FromDays(7),
                        flags: CommandFlags.FireAndForget
                    );

                    if (ackIds.Count > 0)
                    {
                        await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, [.. ackIds]);
                    }
                }
                finally
                {
                    channel.Dispose();
                }
            }
            catch (RpcException rpcException)
            {
                logger.LogWarning(
                    rpcException,
                    "Edge datapoint sync RPC failed with status {StatusCode}: {StatusDetail}",
                    rpcException.StatusCode,
                    rpcException.Status.Detail
                );
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Edge datapoint sync background service failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }

    private static Dictionary<string, string> ParseResult(StreamEntry entry)
    {
        return entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString());
    }
}
