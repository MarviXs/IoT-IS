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
    private const int SyncBatchSize = 10000;
    private const string SyncModeStateKey = "edge:hub:datapoints:sync-mode";
    private const string BackfillCutoffKey = "edge:hub:datapoints:backfill:cutoff-ms";
    private const string BackfillCursorTimestampMsKey = "edge:hub:datapoints:backfill:cursor:timestamp-ms";
    private const string BackfillCursorOffsetKey = "edge:hub:datapoints:backfill:cursor:offset";
    private const string BackfillCompletedKey = "edge:hub:datapoints:backfill:completed";
    private static readonly TimeSpan BackfillStateTtl = TimeSpan.FromDays(30);

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
                var timescaleContext = scope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();
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
                var dataPointSyncMode = NormalizeSyncMode(settings.DataPointSyncMode);
                await EnsureBackfillStateAsync(redis, dataPointSyncMode);

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
                    SyncBatchSize
                );
                var messages = await redis.Db.StreamReadGroupAsync(StreamName, GroupName, consumerName, ">", SyncBatchSize);
                messages = [.. messages, .. autoClaimResult.ClaimedEntries];

                var edgeMetadataVersion = await metadataVersionService.GetCurrentVersionAsync(stoppingToken);
                var client = hubApiClientFactory.Create(settings.HubUrl!, settings.HubToken!);
                var syncCompleted = true;
                var hasSuccessfulDatapointSync = false;

                if (dataPointSyncMode == EdgeDataPointSyncMode.AllDatapoints)
                {
                    var backfillBatch = await BuildTimescaleBackfillBatchAsync(timescaleContext, redis, edgeMetadataVersion, stoppingToken);
                    if (backfillBatch.HasRows)
                    {
                        if (backfillBatch.Request.Datapoints.Count > 0)
                        {
                            var backfillResponse = await SendDatapointBatchWithMetadataRetryAsync(
                                client,
                                backfillBatch.Request,
                                edgeMetadataVersion,
                                metadataSnapshotService,
                                stoppingToken
                            );
                            if (backfillResponse == null || !backfillResponse.DatapointsProcessed)
                            {
                                continue;
                            }

                            hasSuccessfulDatapointSync = true;
                        }

                        if (backfillBatch.LastTimestampUnixMs.HasValue)
                        {
                            await SaveBackfillCursorAsync(
                                redis,
                                backfillBatch.LastTimestampUnixMs.Value,
                                backfillBatch.NextTimestampOffset
                            );
                        }
                    }

                    if (backfillBatch.MarkCompleted)
                    {
                        await MarkBackfillCompletedAsync(redis);
                    }
                }

                var validDataPoints = new List<(RedisValue AckId, HubDataPoint Datapoint)>();
                var invalidAckIds = new List<RedisValue>();

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

                            if (
                                !parsed.TryGetValue("device_id", out var rawDeviceId)
                                || !Guid.TryParse(rawDeviceId, out var deviceId)
                                || !existingDeviceSet.Contains(deviceId)
                            )
                            {
                                invalidAckIds.Add(message.Id);
                                continue;
                            }

                            if (!parsed.TryGetValue("sensor_tag", out var sensorTag) || string.IsNullOrWhiteSpace(sensorTag))
                            {
                                invalidAckIds.Add(message.Id);
                                continue;
                            }

                            if (
                                !parsed.TryGetValue("value", out var rawValue)
                                || !double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                                || double.IsNaN(value)
                                || double.IsInfinity(value)
                            )
                            {
                                invalidAckIds.Add(message.Id);
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

                            validDataPoints.Add(
                                (
                                    message.Id,
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
                                )
                            );
                        }
                        catch (Exception parseException)
                        {
                            logger.LogWarning(parseException, "Failed to parse datapoint stream message for edge hub sync: {MessageId}", message.Id);
                            invalidAckIds.Add(message.Id);
                        }
                    }
                }

                if (validDataPoints.Count == 0)
                {
                    if (!hasSuccessfulDatapointSync)
                    {
                        var emptyBatchResponse = await SendDatapointBatchWithMetadataRetryAsync(
                            client,
                            new SyncDataPointsRequest
                            {
                                EdgeMetadataVersion = edgeMetadataVersion
                            },
                            edgeMetadataVersion,
                            metadataSnapshotService,
                            stoppingToken
                        );
                        if (emptyBatchResponse == null || !emptyBatchResponse.DatapointsProcessed)
                        {
                            continue;
                        }

                        hasSuccessfulDatapointSync = true;
                    }
                }
                else
                {
                    foreach (var datapointBatch in validDataPoints.Chunk(SyncBatchSize))
                    {
                        var batchRequest = new SyncDataPointsRequest
                        {
                            EdgeMetadataVersion = edgeMetadataVersion,
                            Datapoints = datapointBatch.Select(x => x.Datapoint).ToList()
                        };

                        var batchResponse = await SendDatapointBatchWithMetadataRetryAsync(
                            client,
                            batchRequest,
                            edgeMetadataVersion,
                            metadataSnapshotService,
                            stoppingToken
                        );
                        if (batchResponse == null || !batchResponse.DatapointsProcessed)
                        {
                            syncCompleted = false;
                            break;
                        }

                        hasSuccessfulDatapointSync = true;
                        await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, datapointBatch.Select(x => x.AckId).ToArray());
                    }
                }

                delaySeconds = configuredSyncIntervalSeconds;

                if (!syncCompleted || !hasSuccessfulDatapointSync)
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

                if (invalidAckIds.Count > 0)
                {
                    await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, [.. invalidAckIds]);
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

    private async Task<SyncDataPointsResponse?> SendDatapointBatchWithMetadataRetryAsync(
        HttpClient client,
        SyncDataPointsRequest request,
        int edgeMetadataVersion,
        EdgeHubMetadataSnapshotService metadataSnapshotService,
        CancellationToken cancellationToken
    )
    {
        var datapointResponse = await SendDatapointSyncRequestAsync(client, request, cancellationToken);
        if (datapointResponse == null)
        {
            return null;
        }

        if (!datapointResponse.RequiresMetadataSync)
        {
            return datapointResponse;
        }

        var metadataRequest = await metadataSnapshotService.BuildSnapshotAsync(
            edgeMetadataVersion,
            datapointResponse.RequiresFullMetadataSync,
            cancellationToken
        );
        var metadataSyncSucceeded = await SendMetadataSyncRequestAsync(client, metadataRequest, cancellationToken);
        if (!metadataSyncSucceeded)
        {
            return null;
        }

        await metadataSnapshotService.MarkMetadataSyncAppliedAsync(metadataRequest.EdgeMetadataVersion, cancellationToken);
        return await SendDatapointSyncRequestAsync(client, request, cancellationToken);
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

    private async Task EnsureBackfillStateAsync(RedisService redis, EdgeDataPointSyncMode mode)
    {
        var normalizedMode = NormalizeSyncMode(mode);
        var previousModeValue = await redis.Db.StringGetAsync(SyncModeStateKey);
        var previousMode = TryParseSyncMode(previousModeValue);
        await redis.Db.StringSetAsync(
            SyncModeStateKey,
            ((int)normalizedMode).ToString(CultureInfo.InvariantCulture),
            BackfillStateTtl
        );

        if (normalizedMode == EdgeDataPointSyncMode.OnlyNew)
        {
            if (previousMode == EdgeDataPointSyncMode.AllDatapoints)
            {
                await ClearBackfillStateAsync(redis);
            }

            return;
        }

        if (previousMode == EdgeDataPointSyncMode.AllDatapoints && await redis.Db.KeyExistsAsync(BackfillCutoffKey))
        {
            return;
        }

        var cutoffUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture);
        await redis.Db.StringSetAsync(BackfillCutoffKey, cutoffUnixMs, BackfillStateTtl);
        await redis.Db.StringSetAsync(BackfillCursorOffsetKey, "0", BackfillStateTtl);
        await redis.Db.KeyDeleteAsync(BackfillCursorTimestampMsKey);
        await redis.Db.StringSetAsync(BackfillCompletedKey, "0", BackfillStateTtl);
    }

    private static EdgeDataPointSyncMode NormalizeSyncMode(EdgeDataPointSyncMode mode)
    {
        return mode == EdgeDataPointSyncMode.AllDatapoints ? EdgeDataPointSyncMode.AllDatapoints : EdgeDataPointSyncMode.OnlyNew;
    }

    private static EdgeDataPointSyncMode? TryParseSyncMode(RedisValue value)
    {
        return value.HasValue
            && int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            && Enum.IsDefined(typeof(EdgeDataPointSyncMode), parsed)
            ? (EdgeDataPointSyncMode)parsed
            : null;
    }

    private async Task<BackfillBatchResult> BuildTimescaleBackfillBatchAsync(
        TimeScaleDbContext timescaleContext,
        RedisService redis,
        int edgeMetadataVersion,
        CancellationToken cancellationToken
    )
    {
        var completedValue = await redis.Db.StringGetAsync(BackfillCompletedKey);
        if (
            completedValue.HasValue
            && int.TryParse(completedValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var isCompleted)
            && isCompleted == 1
        )
        {
            return BackfillBatchResult.Empty(markCompleted: false);
        }

        var cutoffValue = await redis.Db.StringGetAsync(BackfillCutoffKey);
        if (!cutoffValue.HasValue || !long.TryParse(cutoffValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var cutoffUnixMs))
        {
            return BackfillBatchResult.Empty(markCompleted: false);
        }

        DateTimeOffset? cursorTimestamp = null;
        var cursorTimestampValue = await redis.Db.StringGetAsync(BackfillCursorTimestampMsKey);
        if (
            cursorTimestampValue.HasValue
            && long.TryParse(cursorTimestampValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var cursorTimestampUnixMs)
        )
        {
            cursorTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(cursorTimestampUnixMs);
        }

        var cursorOffset = 0;
        var cursorOffsetValue = await redis.Db.StringGetAsync(BackfillCursorOffsetKey);
        if (cursorOffsetValue.HasValue && int.TryParse(cursorOffsetValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedOffset))
        {
            cursorOffset = Math.Max(0, parsedOffset);
        }

        var cutoffTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(cutoffUnixMs);
        var query = timescaleContext
            .DataPoints.AsNoTracking()
            .Where(dataPoint => dataPoint.TimeStamp <= cutoffTimestamp);

        if (cursorTimestamp.HasValue)
        {
            query = query.Where(dataPoint => dataPoint.TimeStamp >= cursorTimestamp.Value);
        }

        query = query.OrderBy(dataPoint => dataPoint.TimeStamp).ThenBy(dataPoint => dataPoint.DeviceId).ThenBy(dataPoint => dataPoint.SensorTag);

        if (cursorTimestamp.HasValue && cursorOffset > 0)
        {
            query = query.Skip(cursorOffset);
        }

        var rawRows = await query.Take(SyncBatchSize).ToListAsync(cancellationToken);
        if (rawRows.Count == 0)
        {
            return BackfillBatchResult.Empty(markCompleted: true);
        }

        var datapoints = new List<HubDataPoint>(rawRows.Count);
        foreach (var raw in rawRows)
        {
            if (raw.Value is null || double.IsNaN(raw.Value.Value) || double.IsInfinity(raw.Value.Value) || string.IsNullOrWhiteSpace(raw.SensorTag))
            {
                continue;
            }

            datapoints.Add(
                new HubDataPoint
                {
                    DeviceId = raw.DeviceId.ToString(),
                    SensorTag = raw.SensorTag,
                    Value = raw.Value.Value,
                    TimestampUnixMs = raw.TimeStamp.ToUnixTimeMilliseconds(),
                    Latitude = raw.Latitude,
                    Longitude = raw.Longitude,
                    GridX = raw.GridX,
                    GridY = raw.GridY
                }
            );
        }

        var lastTimestamp = rawRows[^1].TimeStamp;
        var rowsAtLastTimestamp = rawRows.Count(row => row.TimeStamp == lastTimestamp);
        var nextOffset = cursorTimestamp.HasValue && cursorTimestamp.Value == lastTimestamp
            ? cursorOffset + rowsAtLastTimestamp
            : rowsAtLastTimestamp;

        return new BackfillBatchResult(
            HasRows: true,
            Request: new SyncDataPointsRequest
            {
                EdgeMetadataVersion = edgeMetadataVersion,
                Datapoints = datapoints
            },
            LastTimestampUnixMs: lastTimestamp.ToUnixTimeMilliseconds(),
            NextTimestampOffset: nextOffset,
            MarkCompleted: rawRows.Count < SyncBatchSize
        );
    }

    private async Task SaveBackfillCursorAsync(RedisService redis, long timestampUnixMs, int offset)
    {
        await redis.Db.StringSetAsync(
            BackfillCursorTimestampMsKey,
            timestampUnixMs.ToString(CultureInfo.InvariantCulture),
            BackfillStateTtl
        );
        await redis.Db.StringSetAsync(
            BackfillCursorOffsetKey,
            offset.ToString(CultureInfo.InvariantCulture),
            BackfillStateTtl
        );
    }

    private async Task MarkBackfillCompletedAsync(RedisService redis)
    {
        await redis.Db.StringSetAsync(BackfillCompletedKey, "1", BackfillStateTtl);
    }

    private async Task ClearBackfillStateAsync(RedisService redis)
    {
        await redis.Db.KeyDeleteAsync(
            [
                (RedisKey)BackfillCutoffKey,
                (RedisKey)BackfillCursorTimestampMsKey,
                (RedisKey)BackfillCursorOffsetKey,
                (RedisKey)BackfillCompletedKey
            ]
        );
    }

    private sealed record BackfillBatchResult(
        bool HasRows,
        SyncDataPointsRequest Request,
        long? LastTimestampUnixMs,
        int NextTimestampOffset,
        bool MarkCompleted
    )
    {
        public static BackfillBatchResult Empty(bool markCompleted)
        {
            return new BackfillBatchResult(
                HasRows: false,
                Request: new SyncDataPointsRequest(),
                LastTimestampUnixMs: null,
                NextTimestampOffset: 0,
                MarkCompleted: markCompleted
            );
        }
    }

    private static Dictionary<string, string> ParseResult(StreamEntry entry)
    {
        return entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString());
    }
}
