using System.Globalization;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Redis;
using Fei.Is.Api.Services.DeviceFirmwares;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Services;

public enum HubEdgeFirmwareDownloadStatus
{
    Success,
    Unauthorized,
    NotFound
}

public class HubEdgeSyncService(
    AppDbContext appContext,
    RedisService redis,
    IHubContext<IsHub, INotificationsClient> hubContext,
    IDeviceFirmwareFileService firmwareFileService
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
        var forceFullSync = await ConsumeForceFullSyncRequestAsync(edgeNode.Id);
        var now = DateTimeOffset.UtcNow;
        if (request.Datapoints.Count == 0)
        {
            return new SyncDataPointsResponse
            {
                NextSyncSeconds = nextSyncSeconds,
                AcceptedCount = 0,
                SkippedCount = 0,
                Hash = await BuildHubSyncHashAsync(cancellationToken),
                ForceFullSync = forceFullSync
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
            SkippedCount = skipped,
            Hash = await BuildHubSyncHashAsync(cancellationToken),
            ForceFullSync = forceFullSync
        };
    }

    public async Task<GetHubSnapshotResponse?> GetHubSnapshotAsync(string edgeToken, CancellationToken cancellationToken)
    {
        var edgeNode = await AuthorizeAndTouchHeartbeatAsync(edgeToken, cancellationToken);
        if (edgeNode == null)
        {
            return null;
        }

        var templates = await appContext
            .DeviceTemplates.AsNoTracking()
            .Include(template => template.Owner)
            .Include(template => template.Sensors)
            .Include(template => template.Commands)
            .Include(template => template.Recipes)
            .ThenInclude(recipe => recipe.Steps)
            .Include(template => template.Controls)
            .Include(template => template.Firmwares)
            .ToListAsync(cancellationToken);

        var devices = await appContext.Devices.AsNoTracking().Include(device => device.Owner).ToListAsync(cancellationToken);

        var response = new GetHubSnapshotResponse();

        foreach (var template in templates)
        {
            var templateMessage = new HubTemplate
            {
                Id = template.Id.ToString(),
                OwnerEmail = template.Owner?.Email ?? string.Empty,
                Name = template.Name,
                DeviceType = (int)template.DeviceType,
                IsGlobal = template.IsGlobal,
                EnableMap = template.EnableMap,
                EnableGrid = template.EnableGrid,
                GridRowSpan = template.GridRowSpan,
                GridColumnSpan = template.GridColumnSpan
            };

            templateMessage.Sensors.AddRange(
                template
                    .Sensors.OrderBy(sensor => sensor.Order)
                    .Select(sensor =>
                        new HubSensor
                        {
                            Id = sensor.Id.ToString(),
                            Tag = sensor.Tag,
                            Name = sensor.Name,
                            Unit = sensor.Unit,
                            AccuracyDecimals = sensor.AccuracyDecimals,
                            Order = sensor.Order,
                            Group = sensor.Group
                        }
                    )
            );

            templateMessage.Commands.AddRange(
                template.Commands.Select(command =>
                    new HubCommand
                    {
                        Id = command.Id.ToString(),
                        DisplayName = command.DisplayName,
                        Name = command.Name,
                        Params = [.. command.Params]
                    }
                )
            );

            templateMessage.Recipes.AddRange(
                template.Recipes.Select(recipe =>
                    new HubRecipe
                    {
                        Id = recipe.Id.ToString(),
                        Name = recipe.Name,
                        Steps =
                        [
                            .. recipe
                                .Steps.OrderBy(step => step.Order)
                                .Select(step =>
                                    new HubRecipeStep
                                    {
                                        Id = step.Id.ToString(),
                                        CommandId = step.CommandId?.ToString(),
                                        SubrecipeId = step.SubrecipeId?.ToString(),
                                        Cycles = step.Cycles,
                                        Order = step.Order
                                    }
                                )
                        ]
                    }
                )
            );

            templateMessage.Controls.AddRange(
                template
                    .Controls.OrderBy(control => control.Order)
                    .Select(control =>
                        new HubDeviceControl
                        {
                            Id = control.Id.ToString(),
                            Name = control.Name,
                            Color = control.Color,
                            Type = (int)control.Type,
                            RecipeId = control.RecipeId?.ToString(),
                            Cycles = control.Cycles,
                            IsInfinite = control.IsInfinite,
                            Order = control.Order,
                            RecipeOnId = control.RecipeOnId?.ToString(),
                            RecipeOffId = control.RecipeOffId?.ToString(),
                            SensorId = control.SensorId?.ToString()
                        }
                    )
            );

            templateMessage.Firmwares.AddRange(
                template.Firmwares.Select(firmware =>
                    new HubFirmware
                    {
                        Id = firmware.Id.ToString(),
                        VersionNumber = firmware.VersionNumber,
                        IsActive = firmware.IsActive,
                        OriginalFileName = firmware.OriginalFileName,
                        StoredFileName = firmware.StoredFileName
                    }
                )
            );

            response.Templates.Add(templateMessage);
        }

        response.Devices.AddRange(
            devices.Select(device =>
                new HubDevice
                {
                    Id = device.Id.ToString(),
                    OwnerEmail = device.Owner?.Email ?? string.Empty,
                    Name = device.Name,
                    AccessToken = device.AccessToken,
                    TemplateId = device.DeviceTemplateId?.ToString(),
                    Protocol = (int)device.Protocol,
                    DataPointRetentionDays = device.DataPointRetentionDays,
                    SampleRateSeconds = device.SampleRateSeconds,
                    CurrentFirmwareVersion = string.IsNullOrWhiteSpace(device.CurrentFirmwareVersion)
                        ? null
                        : device.CurrentFirmwareVersion,
                    Mac = string.IsNullOrWhiteSpace(device.Mac) ? null : device.Mac
                }
            )
        );

        return response;
    }

    private async Task<string> BuildHubSyncHashAsync(CancellationToken cancellationToken)
    {
        var signatures = new[]
        {
            await BuildTableSignatureAsync(appContext.DeviceTemplates.AsNoTracking(), cancellationToken),
            await BuildTableSignatureAsync(appContext.Sensors.AsNoTracking(), cancellationToken),
            await BuildTableSignatureAsync(appContext.Commands.AsNoTracking(), cancellationToken),
            await BuildTableSignatureAsync(appContext.Recipes.AsNoTracking(), cancellationToken),
            await BuildTableSignatureAsync(appContext.RecipeSteps.AsNoTracking(), cancellationToken),
            await BuildTableSignatureAsync(appContext.DeviceControls.AsNoTracking(), cancellationToken),
            await BuildTableSignatureAsync(appContext.DeviceFirmwares.AsNoTracking(), cancellationToken)
        };

        return ComputeHash(signatures);
    }

    public async Task<(HubEdgeFirmwareDownloadStatus Status, Stream? Stream, string? FileName)> DownloadFirmwareAsync(
        Guid templateId,
        Guid firmwareId,
        string edgeToken,
        CancellationToken cancellationToken
    )
    {
        var edgeNode = await AuthorizeAndTouchHeartbeatAsync(edgeToken, cancellationToken);
        if (edgeNode == null)
        {
            return (HubEdgeFirmwareDownloadStatus.Unauthorized, null, null);
        }

        var firmware = await appContext
            .DeviceFirmwares.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == firmwareId && f.DeviceTemplateId == templateId, cancellationToken);

        if (firmware == null)
        {
            return (HubEdgeFirmwareDownloadStatus.NotFound, null, null);
        }

        var stream = await firmwareFileService.OpenReadAsync(firmware.StoredFileName, cancellationToken);
        return (HubEdgeFirmwareDownloadStatus.Success, stream, firmware.OriginalFileName);
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

    private async Task<bool> ConsumeForceFullSyncRequestAsync(Guid edgeNodeId)
    {
        var key = EdgeSyncNowRedisKeys.BuildForceFullSyncKey(edgeNodeId);
        var value = await redis.Db.StringGetAsync(key);
        if (!value.HasValue)
        {
            return false;
        }

        _ = redis.Db.KeyDeleteAsync(key, flags: CommandFlags.FireAndForget);
        return true;
    }

    private static async Task<TableSignature> BuildTableSignatureAsync<TEntity>(IQueryable<TEntity> query, CancellationToken cancellationToken)
        where TEntity : BaseModel
    {
        var aggregate = await query
            .GroupBy(_ => 1)
            .Select(group => new { Count = group.Count(), MaxUpdatedAt = (DateTime?)group.Max(item => item.UpdatedAt) })
            .FirstOrDefaultAsync(cancellationToken);

        return new TableSignature(typeof(TEntity).Name, aggregate?.Count ?? 0, aggregate?.MaxUpdatedAt?.Ticks ?? 0);
    }

    private static string ComputeHash(IEnumerable<TableSignature> signatures)
    {
        using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Span<byte> intBuffer = stackalloc byte[4];
        Span<byte> longBuffer = stackalloc byte[8];
        foreach (var signature in signatures)
        {
            var tableNameBytes = Encoding.UTF8.GetBytes(signature.TableName);

            BinaryPrimitives.WriteInt32LittleEndian(intBuffer, tableNameBytes.Length);
            hasher.AppendData(intBuffer);
            hasher.AppendData(tableNameBytes);

            BinaryPrimitives.WriteInt32LittleEndian(intBuffer, signature.Count);
            hasher.AppendData(intBuffer);

            BinaryPrimitives.WriteInt64LittleEndian(longBuffer, signature.MaxUpdatedAtTicks);
            hasher.AppendData(longBuffer);
        }

        return Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
    }

    private sealed record TableSignature(string TableName, int Count, long MaxUpdatedAtTicks);
}
