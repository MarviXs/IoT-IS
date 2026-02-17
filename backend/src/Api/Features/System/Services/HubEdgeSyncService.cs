using System.Globalization;
using System.Text.Json;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using Fei.Is.Api.Features.DataPoints.Queries;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Services;

public class HubEdgeSyncService(
    AppDbContext appContext,
    RedisService redis,
    IHubContext<IsHub, INotificationsClient> hubContext
)
{
    private const int DefaultNextSyncSeconds = 5;

    public async Task<SyncDataPointsResponse?> SyncDataPointsAsync(SyncDataPointsRequest request, string edgeToken, CancellationToken cancellationToken)
    {
        var edgeNode = await AuthorizeAndTouchHeartbeatAsync(edgeToken, cancellationToken);
        if (edgeNode == null)
        {
            return null;
        }

        var nextSyncSeconds = edgeNode.UpdateRateSeconds > 0 ? edgeNode.UpdateRateSeconds : DefaultNextSyncSeconds;
        var now = DateTimeOffset.UtcNow;
        if (request.Datapoints.Count == 0)
        {
            return new SyncDataPointsResponse
            {
                NextSyncSeconds = nextSyncSeconds,
                AcceptedCount = 0,
                SkippedCount = 0
            };
        }

        var candidateIds = request
            .Datapoints.Select(dataPoint => Guid.TryParse(dataPoint.DeviceId, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var existingDevices = await appContext.Devices.Where(device => candidateIds.Contains(device.Id)).ToListAsync(cancellationToken);
        var existingIdSet = existingDevices.Select(device => device.Id).ToHashSet();

        var hasSourceMarkerChanges = false;
        foreach (var device in existingDevices)
        {
            if (!device.SyncedFromEdge || device.SyncedFromEdgeNodeId != edgeNode.Id)
            {
                device.SyncedFromEdge = true;
                device.SyncedFromEdgeNodeId = edgeNode.Id;
                hasSourceMarkerChanges = true;
            }
        }

        if (hasSourceMarkerChanges)
        {
            await appContext.SaveChangesAsync(cancellationToken);
        }

        var accepted = 0;
        var skipped = 0;

        foreach (var incoming in request.Datapoints)
        {
            if (!Guid.TryParse(incoming.DeviceId, out var deviceId) || !existingIdSet.Contains(deviceId))
            {
                skipped++;
                continue;
            }

            if (double.IsNaN(incoming.Value) || double.IsInfinity(incoming.Value))
            {
                skipped++;
                continue;
            }

            DateTimeOffset timestamp;
            try
            {
                timestamp = incoming.TimestampUnixMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(incoming.TimestampUnixMs) : now;
            }
            catch (ArgumentOutOfRangeException)
            {
                timestamp = now;
            }

            var deviceIdString = deviceId.ToString();
            var streamEntries = new List<NameValueEntry>
            {
                new("device_id", deviceIdString),
                new("sensor_tag", incoming.SensorTag),
                new("value", incoming.Value.ToString(CultureInfo.InvariantCulture)),
                new("timestamp", timestamp.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture))
            };

            if (incoming.HasLatitude)
            {
                streamEntries.Add(new("latitude", incoming.Latitude!.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (incoming.HasLongitude)
            {
                streamEntries.Add(new("longitude", incoming.Longitude!.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (incoming.HasGridX)
            {
                streamEntries.Add(new("grid_x", incoming.GridX!.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (incoming.HasGridY)
            {
                streamEntries.Add(new("grid_y", incoming.GridY!.Value.ToString(CultureInfo.InvariantCulture)));
            }

            await redis.Db.StreamAddAsync("datapoints", streamEntries.ToArray(), maxLength: 500000);
            await redis.Db.StringSetAsync($"device:{deviceIdString}:connected", "1", TimeSpan.FromMinutes(30), flags: CommandFlags.FireAndForget);
            await redis.Db.StringSetAsync(
                $"device:{deviceIdString}:lastSeen",
                now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                flags: CommandFlags.FireAndForget
            );

            var latest = new GetLatestDataPoints.Response(
                timestamp,
                incoming.Value,
                incoming.HasLatitude ? incoming.Latitude : null,
                incoming.HasLongitude ? incoming.Longitude : null,
                incoming.HasGridX ? incoming.GridX : null,
                incoming.HasGridY ? incoming.GridY : null
            );
            await redis.Db.StringSetAsync(
                $"device:{deviceIdString}:{incoming.SensorTag}:last",
                JsonSerializer.Serialize(latest),
                TimeSpan.FromHours(1),
                flags: CommandFlags.FireAndForget
            );

            await hubContext
                .Clients.Group(deviceIdString)
                .ReceiveSensorLastDataPoint(
                    new SensorLastDataPointDto(
                        deviceIdString,
                        incoming.SensorTag,
                        incoming.Value,
                        incoming.HasLatitude ? incoming.Latitude : null,
                        incoming.HasLongitude ? incoming.Longitude : null,
                        incoming.HasGridX ? incoming.GridX : null,
                        incoming.HasGridY ? incoming.GridY : null,
                        timestamp
                    )
                );

            accepted++;
        }

        return new SyncDataPointsResponse
        {
            NextSyncSeconds = nextSyncSeconds,
            AcceptedCount = accepted,
            SkippedCount = skipped
        };
    }

    private async Task<EdgeNode?> AuthorizeAndTouchHeartbeatAsync(string token, CancellationToken cancellationToken)
    {
        var normalized = token.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var edgeNode = await appContext.EdgeNodes.AsNoTracking().FirstOrDefaultAsync(node => node.Token == normalized, cancellationToken);
        if (edgeNode == null)
        {
            return null;
        }

        await redis.Db.StringSetAsync(
            $"edge-node:{edgeNode.Id}:last-sync",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            TimeSpan.FromDays(7),
            flags: CommandFlags.FireAndForget
        );

        return edgeNode;
    }
}
