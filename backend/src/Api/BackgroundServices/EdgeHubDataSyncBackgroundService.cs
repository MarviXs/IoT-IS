using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.System.Services;
using Fei.Is.Api.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.BackgroundServices;

public class EdgeHubDataSyncBackgroundService(IServiceProvider serviceProvider, ILogger<EdgeHubDataSyncBackgroundService> logger)
    : BackgroundService
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
                var hubApiClientFactory = scope.ServiceProvider.GetRequiredService<HubApiClientFactory>();
                var metadataVersionService = scope.ServiceProvider.GetRequiredService<EdgeMetadataVersionService>();
                var metadataSnapshotService = scope.ServiceProvider.GetRequiredService<EdgeHubMetadataSnapshotService>();

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
                var configuredSyncIntervalSeconds = settings.SyncIntervalSeconds > 0 ? settings.SyncIntervalSeconds : DefaultSyncSeconds;
                delaySeconds = configuredSyncIntervalSeconds;

                if (
                    !await redis.Db.KeyExistsAsync(StreamName)
                    || (await redis.Db.StreamGroupInfoAsync(StreamName)).All(group => group.Name != GroupName)
                )
                {
                    await redis.Db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "$", true);
                }

                var autoClaimResult = await redis.Db.StreamAutoClaimAsync(
                    StreamName,
                    GroupName,
                    consumerName,
                    MaxPendingTimeUnclaimed,
                    "0-0",
                    5000
                );
                var messages = await redis.Db.StreamReadGroupAsync(StreamName, GroupName, consumerName, ">");
                messages = [.. messages, .. autoClaimResult.ClaimedEntries];

                var edgeMetadataVersion = await metadataVersionService.GetCurrentVersionAsync(stoppingToken);
                var request = new SyncDataPointsRequest
                {
                    EdgeMetadataVersion = edgeMetadataVersion
                };
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

                    var existingDeviceIds = await appContext
                        .Devices.AsNoTracking()
                        .Where(device => candidateDeviceIds.Contains(device.Id))
                        .Select(device => device.Id)
                        .ToListAsync(stoppingToken);
                    var existingDeviceSet = existingDeviceIds.ToHashSet();

                    foreach (var message in messages)
                    {
                        try
                        {
                            var parsed = ParseResult(message);
                            ackIds.Add(message.Id);

                            if (
                                !parsed.TryGetValue("device_id", out var rawDeviceId)
                                || !Guid.TryParse(rawDeviceId, out var deviceId)
                                || !existingDeviceSet.Contains(deviceId)
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

                            double? latitude = null;
                            double? longitude = null;
                            int? gridX = null;
                            int? gridY = null;

                            if (
                                parsed.TryGetValue("latitude", out var rawLatitude)
                                && double.TryParse(rawLatitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedLatitude)
                            )
                            {
                                latitude = parsedLatitude;
                            }

                            if (
                                parsed.TryGetValue("longitude", out var rawLongitude)
                                && double.TryParse(rawLongitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedLongitude)
                            )
                            {
                                longitude = parsedLongitude;
                            }

                            if (
                                parsed.TryGetValue("grid_x", out var rawGridX)
                                && int.TryParse(rawGridX, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedGridX)
                            )
                            {
                                gridX = parsedGridX;
                            }

                            if (
                                parsed.TryGetValue("grid_y", out var rawGridY)
                                && int.TryParse(rawGridY, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedGridY)
                            )
                            {
                                gridY = parsedGridY;
                            }

                            request.Datapoints.Add(
                                new HubDataPoint
                                {
                                    DeviceId = deviceId.ToString(),
                                    SensorTag = sensorTag,
                                    Value = value,
                                    TimestampUnixMs = timestampUnixMs,
                                    Latitude = latitude,
                                    Longitude = longitude,
                                    GridX = gridX,
                                    GridY = gridY
                                }
                            );
                        }
                        catch (Exception parseException)
                        {
                            logger.LogWarning(parseException, "Failed to parse datapoint stream message for edge hub sync: {MessageId}", message.Id);
                        }
                    }
                }

                var client = hubApiClientFactory.Create(settings.HubUrl!, settings.HubToken!);
                var datapointResponse = await SendDatapointSyncRequestAsync(client, request, stoppingToken);
                if (datapointResponse == null)
                {
                    continue;
                }

                if (datapointResponse.RequiresMetadataSync)
                {
                    var metadataRequest = await metadataSnapshotService.BuildSnapshotAsync(
                        edgeMetadataVersion,
                        datapointResponse.RequiresFullMetadataSync,
                        stoppingToken
                    );
                    var metadataSyncSucceeded = await SendMetadataSyncRequestAsync(client, metadataRequest, stoppingToken);
                    if (!metadataSyncSucceeded)
                    {
                        continue;
                    }

                    await metadataSnapshotService.MarkMetadataSyncAppliedAsync(metadataRequest.EdgeMetadataVersion, stoppingToken);

                    datapointResponse = await SendDatapointSyncRequestAsync(client, request, stoppingToken);
                    if (datapointResponse == null)
                    {
                        continue;
                    }
                }

                delaySeconds = configuredSyncIntervalSeconds;

                if (!datapointResponse.DatapointsProcessed)
                {
                    continue;
                }

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
            catch (HttpRequestException requestException)
            {
                logger.LogWarning(requestException, "Edge datapoint sync HTTP request failed: {Message}", requestException.Message);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Edge datapoint sync background service failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }

    private async Task<SyncDataPointsResponse?> SendDatapointSyncRequestAsync(
        HttpClient client,
        SyncDataPointsRequest request,
        CancellationToken cancellationToken
    )
    {
        using var responseMessage = await client.PostAsJsonAsync("system/hub-sync/datapoints", request, cancellationToken);
        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Edge datapoint sync request unauthorized by hub endpoint");
            return null;
        }

        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadFromJsonAsync<SyncDataPointsResponse>(cancellationToken: cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("Hub datapoint sync response is empty.");
        }

        return response;
    }

    private async Task<bool> SendMetadataSyncRequestAsync(HttpClient client, SyncMetadataRequest request, CancellationToken cancellationToken)
    {
        using var responseMessage = await client.PostAsJsonAsync("system/hub-sync/metadata", request, cancellationToken);
        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Edge metadata sync request unauthorized by hub endpoint");
            return false;
        }

        responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadFromJsonAsync<SyncMetadataResponse>(cancellationToken: cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("Hub metadata sync response is empty.");
        }

        return true;
    }

    private static Dictionary<string, string> ParseResult(StreamEntry entry)
    {
        return entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString());
    }
}
