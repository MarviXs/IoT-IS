using System.Globalization;
using System.Text.Json;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
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
    private const int MetadataVersionTtlDays = 30;

    public async Task<SyncDataPointsResponse?> SyncDataPointsAsync(SyncDataPointsRequest request, string edgeToken, CancellationToken cancellationToken)
    {
        var edgeNode = await AuthorizeAndTouchHeartbeatAsync(edgeToken, cancellationToken);
        if (edgeNode == null)
        {
            return null;
        }

        var configuredSyncIntervalSeconds = await appContext
            .SystemNodeSettings.AsNoTracking()
            .OrderBy(setting => setting.CreatedAt)
            .Select(setting => (int?)setting.SyncIntervalSeconds)
            .FirstOrDefaultAsync(cancellationToken);
        var nextSyncSeconds = configuredSyncIntervalSeconds.HasValue && configuredSyncIntervalSeconds.Value > 0
            ? configuredSyncIntervalSeconds.Value
            : DefaultNextSyncSeconds;

        await redis.Db.StringSetAsync(
            $"edge-node:{edgeNode.Id}:expected-sync-seconds",
            nextSyncSeconds.ToString(CultureInfo.InvariantCulture),
            TimeSpan.FromDays(7),
            flags: CommandFlags.FireAndForget
        );

        var incomingVersion = NormalizeVersion(request.EdgeMetadataVersion);
        var appliedVersion = await GetAppliedMetadataVersionAsync(edgeNode.Id);
        if (!appliedVersion.HasValue)
        {
            return new SyncDataPointsResponse
            {
                NextSyncSeconds = nextSyncSeconds,
                AcceptedCount = 0,
                SkippedCount = 0,
                RequiresMetadataSync = true,
                RequiresFullMetadataSync = true,
                DatapointsProcessed = false
            };
        }

        if (incomingVersion != appliedVersion.Value)
        {
            return new SyncDataPointsResponse
            {
                NextSyncSeconds = nextSyncSeconds,
                AcceptedCount = 0,
                SkippedCount = 0,
                RequiresMetadataSync = true,
                RequiresFullMetadataSync = false,
                DatapointsProcessed = false
            };
        }

        var now = DateTimeOffset.UtcNow;
        if (request.Datapoints.Count == 0)
        {
            return new SyncDataPointsResponse
            {
                NextSyncSeconds = nextSyncSeconds,
                AcceptedCount = 0,
                SkippedCount = 0,
                RequiresMetadataSync = false,
                DatapointsProcessed = true
            };
        }

        var candidateIds = request
            .Datapoints.Select(dataPoint => Guid.TryParse(dataPoint.DeviceId, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var existingIdSet = (await appContext
                .Devices.AsNoTracking()
                .Where(device => candidateIds.Contains(device.Id) && device.SyncedFromEdge && device.SyncedFromEdgeNodeId == edgeNode.Id)
                .Select(device => device.Id)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var accepted = 0;
        var skipped = 0;
        var streamEntriesBatch = new List<NameValueEntry[]>(request.Datapoints.Count);
        var latestBySensorKey = new Dictionary<string, string>(StringComparer.Ordinal);
        var touchedDevices = new HashSet<string>();
        var notifications = new List<SensorLastDataPointDto>(request.Datapoints.Count);

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

            streamEntriesBatch.Add(streamEntries.ToArray());
            touchedDevices.Add(deviceIdString);

            var latest = new GetLatestDataPoints.Response(
                timestamp,
                incoming.Value,
                incoming.HasLatitude ? incoming.Latitude : null,
                incoming.HasLongitude ? incoming.Longitude : null,
                incoming.HasGridX ? incoming.GridX : null,
                incoming.HasGridY ? incoming.GridY : null
            );
            latestBySensorKey[$"device:{deviceIdString}:{incoming.SensorTag}:last"] = JsonSerializer.Serialize(latest);
            notifications.Add(
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

        if (accepted > 0)
        {
            var nowUnixSeconds = now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
            var redisTasks = new List<Task>(streamEntriesBatch.Count + touchedDevices.Count * 2 + latestBySensorKey.Count);

            foreach (var streamEntries in streamEntriesBatch)
            {
                redisTasks.Add(redis.Db.StreamAddAsync("datapoints", streamEntries, maxLength: 500000));
            }

            foreach (var deviceIdString in touchedDevices)
            {
                redisTasks.Add(
                    redis.Db.StringSetAsync(
                        $"device:{deviceIdString}:connected",
                        "1",
                        TimeSpan.FromMinutes(30),
                        flags: CommandFlags.FireAndForget
                    )
                );
                redisTasks.Add(
                    redis.Db.StringSetAsync(
                        $"device:{deviceIdString}:lastSeen",
                        nowUnixSeconds,
                        flags: CommandFlags.FireAndForget
                    )
                );
            }

            foreach (var latestEntry in latestBySensorKey)
            {
                redisTasks.Add(
                    redis.Db.StringSetAsync(
                        latestEntry.Key,
                        latestEntry.Value,
                        TimeSpan.FromHours(1),
                        flags: CommandFlags.FireAndForget
                    )
                );
            }

            await Task.WhenAll(redisTasks);

            await Task.WhenAll(notifications.Select(notification =>
                hubContext.Clients.Group(notification.DeviceId).ReceiveSensorLastDataPoint(notification)));
        }

        return new SyncDataPointsResponse
        {
            NextSyncSeconds = nextSyncSeconds,
            AcceptedCount = accepted,
            SkippedCount = skipped,
            RequiresMetadataSync = false,
            DatapointsProcessed = true
        };
    }

    public async Task<SyncMetadataResponse?> SyncMetadataAsync(SyncMetadataRequest request, string edgeToken, CancellationToken cancellationToken)
    {
        var edgeNode = await AuthorizeAndTouchHeartbeatAsync(edgeToken, cancellationToken);
        if (edgeNode == null)
        {
            return null;
        }

        var incomingVersion = NormalizeVersion(request.EdgeMetadataVersion);

        var summary = new MetadataSummaryAccumulator();
        var isFullSnapshot = request.IsFullSnapshot;
        var incomingTemplateIds = request.Templates.Select(template => template.Id).ToHashSet();
        var incomingDeviceIds = request.Devices.Select(device => device.Id).ToHashSet();
        var deletedTemplateIds = request.DeletedTemplateIds.ToHashSet();
        var deletedDeviceIds = request.DeletedDeviceIds.ToHashSet();
        var templateIdsToLoad = isFullSnapshot ? incomingTemplateIds : incomingTemplateIds.Concat(deletedTemplateIds).ToHashSet();

        await using var transaction = await appContext.Database.BeginTransactionAsync(cancellationToken);

        var ownerLookup = await BuildOwnerLookupAsync(request, cancellationToken);

        var trackedTemplates = await appContext
            .DeviceTemplates.Include(template => template.Sensors)
            .Include(template => template.Commands)
            .Include(template => template.Controls)
            .Include(template => template.Recipes)
            .ThenInclude(recipe => recipe.Steps)
            .Where(template =>
                templateIdsToLoad.Contains(template.Id)
                || (isFullSnapshot && template.SyncedFromEdge && template.SyncedFromEdgeNodeId == edgeNode.Id)
            )
            .ToListAsync(cancellationToken);
        var templateById = trackedTemplates.ToDictionary(template => template.Id);
        var syncedTemplatesFromEdge = trackedTemplates
            .Where(template => template.SyncedFromEdge && template.SyncedFromEdgeNodeId == edgeNode.Id)
            .ToDictionary(template => template.Id);

        var appliedTemplateIds = new HashSet<Guid>();
        foreach (var payload in request.Templates)
        {
            var owner = ResolveOwner(payload.OwnerEmail, ownerLookup);
            if (owner == null)
            {
                summary.SkippedOwners++;
                summary.TemplatesSkipped++;
                continue;
            }

            DeviceTemplate template;
            var templateAlreadyExists = false;
            if (templateById.TryGetValue(payload.Id, out var existingTemplate))
            {
                if (!existingTemplate.SyncedFromEdge || existingTemplate.SyncedFromEdgeNodeId != edgeNode.Id)
                {
                    summary.TemplatesConflicts++;
                    continue;
                }

                template = existingTemplate;
                templateAlreadyExists = true;
                summary.TemplatesUpdated++;
            }
            else
            {
                template = new DeviceTemplate
                {
                    Id = payload.Id,
                    Name = payload.Name.Trim()
                };
                await appContext.DeviceTemplates.AddAsync(template, cancellationToken);
                templateById[payload.Id] = template;
                summary.TemplatesCreated++;
            }

            template.Name = payload.Name.Trim();
            template.DeviceType = payload.DeviceType;
            template.IsGlobal = payload.IsGlobal;
            template.EnableMap = payload.EnableMap;
            template.EnableGrid = payload.EnableGrid;
            template.GridRowSpan = payload.EnableGrid ? payload.GridRowSpan : null;
            template.GridColumnSpan = payload.EnableGrid ? payload.GridColumnSpan : null;
            template.OwnerId = owner.Id;
            template.Owner = owner;
            template.SyncedFromEdge = true;
            template.SyncedFromEdgeNodeId = edgeNode.Id;

            var removedSensorsCount = template.Sensors.Count;
            summary.SensorsDeleted += removedSensorsCount;
            appContext.Sensors.RemoveRange(template.Sensors);

            var removedCommandsCount = template.Commands.Count;
            summary.CommandsDeleted += removedCommandsCount;
            appContext.Commands.RemoveRange(template.Commands);

            var removedControlsCount = template.Controls.Count;
            summary.ControlsDeleted += removedControlsCount;
            appContext.DeviceControls.RemoveRange(template.Controls);

            var removedRecipeStepsCount = template.Recipes.Sum(recipe => recipe.Steps.Count);
            var removedRecipesCount = template.Recipes.Count;
            summary.RecipeStepsDeleted += removedRecipeStepsCount;
            summary.RecipesDeleted += removedRecipesCount;
            appContext.Recipes.RemoveRange(template.Recipes);

            if (templateAlreadyExists && removedSensorsCount + removedCommandsCount + removedControlsCount + removedRecipesCount > 0)
            {
                await appContext.SaveChangesAsync(cancellationToken);
            }

            var sensorsById = new Dictionary<Guid, Sensor>();
            foreach (var sensorPayload in payload.Sensors)
            {
                if (sensorsById.ContainsKey(sensorPayload.Id))
                {
                    continue;
                }

                var sensor = new Sensor
                {
                    Id = sensorPayload.Id,
                    DeviceTemplateId = template.Id,
                    Name = sensorPayload.Name.Trim(),
                    Tag = sensorPayload.Tag.Trim(),
                    Unit = sensorPayload.Unit,
                    AccuracyDecimals = sensorPayload.AccuracyDecimals,
                    Order = sensorPayload.Order,
                    Group = sensorPayload.Group
                };

                sensorsById[sensor.Id] = sensor;
                appContext.Sensors.Add(sensor);
                summary.SensorsUpserted++;
            }

            var commandsById = new Dictionary<Guid, Command>();
            foreach (var commandPayload in payload.Commands)
            {
                if (commandsById.ContainsKey(commandPayload.Id))
                {
                    continue;
                }

                var command = new Command
                {
                    Id = commandPayload.Id,
                    DeviceTemplateId = template.Id,
                    DisplayName = commandPayload.DisplayName.Trim(),
                    Name = commandPayload.Name.Trim(),
                    Params = [.. commandPayload.Params]
                };

                commandsById[command.Id] = command;
                appContext.Commands.Add(command);
                summary.CommandsUpserted++;
            }

            var recipesById = new Dictionary<Guid, Recipe>();
            foreach (var recipePayload in payload.Recipes)
            {
                if (recipesById.ContainsKey(recipePayload.Id))
                {
                    continue;
                }

                var recipe = new Recipe
                {
                    Id = recipePayload.Id,
                    DeviceTemplateId = template.Id,
                    Name = recipePayload.Name.Trim()
                };
                recipesById[recipe.Id] = recipe;
                appContext.Recipes.Add(recipe);
                summary.RecipesUpserted++;
            }

            foreach (var recipePayload in payload.Recipes)
            {
                if (!recipesById.TryGetValue(recipePayload.Id, out var recipe))
                {
                    continue;
                }

                foreach (var stepPayload in recipePayload.Steps)
                {
                    if (stepPayload.CommandId.HasValue == stepPayload.SubrecipeId.HasValue)
                    {
                        continue;
                    }

                    if (stepPayload.CommandId.HasValue && !commandsById.ContainsKey(stepPayload.CommandId.Value))
                    {
                        continue;
                    }

                    if (stepPayload.SubrecipeId.HasValue && !recipesById.ContainsKey(stepPayload.SubrecipeId.Value))
                    {
                        continue;
                    }

                    var step = new RecipeStep
                    {
                        Id = stepPayload.Id,
                        RecipeId = recipe.Id,
                        CommandId = stepPayload.CommandId,
                        SubrecipeId = stepPayload.SubrecipeId,
                        Cycles = stepPayload.Cycles,
                        Order = stepPayload.Order
                    };
                    appContext.RecipeSteps.Add(step);
                    summary.RecipeStepsUpserted++;
                }
            }

            foreach (var controlPayload in payload.Controls)
            {
                var control = new DeviceControl
                {
                    Id = controlPayload.Id,
                    DeviceTemplateId = template.Id,
                    Name = controlPayload.Name.Trim(),
                    Color = controlPayload.Color.Trim(),
                    Type = controlPayload.Type,
                    RecipeId = controlPayload.RecipeId.HasValue && recipesById.ContainsKey(controlPayload.RecipeId.Value)
                        ? controlPayload.RecipeId
                        : null,
                    RecipeOnId = controlPayload.RecipeOnId.HasValue && recipesById.ContainsKey(controlPayload.RecipeOnId.Value)
                        ? controlPayload.RecipeOnId
                        : null,
                    RecipeOffId = controlPayload.RecipeOffId.HasValue && recipesById.ContainsKey(controlPayload.RecipeOffId.Value)
                        ? controlPayload.RecipeOffId
                        : null,
                    SensorId = controlPayload.SensorId.HasValue && sensorsById.ContainsKey(controlPayload.SensorId.Value)
                        ? controlPayload.SensorId
                        : null,
                    Cycles = controlPayload.Cycles,
                    IsInfinite = controlPayload.IsInfinite,
                    Order = controlPayload.Order
                };
                appContext.DeviceControls.Add(control);
                summary.ControlsUpserted++;
            }

            appliedTemplateIds.Add(template.Id);
        }

        var templatesToDelete = isFullSnapshot
            ? syncedTemplatesFromEdge.Values.Where(template => !incomingTemplateIds.Contains(template.Id)).ToList()
            : syncedTemplatesFromEdge.Values.Where(template => deletedTemplateIds.Contains(template.Id)).ToList();
        if (templatesToDelete.Count > 0)
        {
            summary.TemplatesDeleted += templatesToDelete.Count;
            summary.SensorsDeleted += templatesToDelete.Sum(template => template.Sensors.Count);
            summary.CommandsDeleted += templatesToDelete.Sum(template => template.Commands.Count);
            summary.ControlsDeleted += templatesToDelete.Sum(template => template.Controls.Count);
            summary.RecipeStepsDeleted += templatesToDelete.Sum(template => template.Recipes.Sum(recipe => recipe.Steps.Count));
            summary.RecipesDeleted += templatesToDelete.Sum(template => template.Recipes.Count);
            appContext.DeviceTemplates.RemoveRange(templatesToDelete);
        }

        var deviceIdsToLoad = isFullSnapshot ? incomingDeviceIds : incomingDeviceIds.Concat(deletedDeviceIds).ToHashSet();
        var trackedDevices = await appContext
            .Devices.Where(device => deviceIdsToLoad.Contains(device.Id) || (isFullSnapshot && device.SyncedFromEdge && device.SyncedFromEdgeNodeId == edgeNode.Id))
            .ToListAsync(cancellationToken);
        var deviceById = trackedDevices.ToDictionary(device => device.Id);
        var syncedDevicesFromEdge = trackedDevices
            .Where(device => device.SyncedFromEdge && device.SyncedFromEdgeNodeId == edgeNode.Id)
            .ToDictionary(device => device.Id);

        var existingTemplateReferenceIds = new HashSet<Guid>();
        if (!isFullSnapshot)
        {
            var referencedTemplateIds = request
                .Devices.Where(device => device.DeviceTemplateId.HasValue && !appliedTemplateIds.Contains(device.DeviceTemplateId.Value))
                .Select(device => device.DeviceTemplateId!.Value)
                .Distinct()
                .ToList();

            if (referencedTemplateIds.Count > 0)
            {
                existingTemplateReferenceIds = (
                    await appContext
                        .DeviceTemplates.AsNoTracking()
                        .Where(template =>
                            referencedTemplateIds.Contains(template.Id)
                            && template.SyncedFromEdge
                            && template.SyncedFromEdgeNodeId == edgeNode.Id
                        )
                        .Select(template => template.Id)
                        .ToListAsync(cancellationToken)
                ).ToHashSet();
            }
        }

        foreach (var payload in request.Devices)
        {
            var owner = ResolveOwner(payload.OwnerEmail, ownerLookup);
            if (owner == null)
            {
                summary.SkippedOwners++;
                summary.DevicesSkipped++;
                continue;
            }

            if (
                payload.DeviceTemplateId.HasValue
                && !appliedTemplateIds.Contains(payload.DeviceTemplateId.Value)
                && (isFullSnapshot || !existingTemplateReferenceIds.Contains(payload.DeviceTemplateId.Value))
            )
            {
                summary.DevicesSkipped++;
                summary.MissingTemplateReferences++;
                continue;
            }

            Device device;
            if (deviceById.TryGetValue(payload.Id, out var existingDevice))
            {
                if (!existingDevice.SyncedFromEdge || existingDevice.SyncedFromEdgeNodeId != edgeNode.Id)
                {
                    summary.DevicesConflicts++;
                    continue;
                }

                var accessTokenConflict = await appContext
                    .Devices.AsNoTracking()
                    .AnyAsync(candidate => candidate.Id != payload.Id && candidate.AccessToken == payload.AccessToken, cancellationToken);
                if (accessTokenConflict)
                {
                    summary.DevicesConflicts++;
                    continue;
                }

                device = existingDevice;
                summary.DevicesUpdated++;
            }
            else
            {
                var accessTokenConflict = await appContext
                    .Devices.AsNoTracking()
                    .AnyAsync(candidate => candidate.AccessToken == payload.AccessToken && candidate.Id != payload.Id, cancellationToken);
                if (accessTokenConflict)
                {
                    summary.DevicesConflicts++;
                    continue;
                }

                device = new Device
                {
                    Id = payload.Id,
                    Name = payload.Name.Trim(),
                    AccessToken = payload.AccessToken
                };
                await appContext.Devices.AddAsync(device, cancellationToken);
                deviceById[device.Id] = device;
                summary.DevicesCreated++;
            }

            device.OwnerId = owner.Id;
            device.Owner = owner;
            device.Name = payload.Name.Trim();
            device.Mac = payload.Mac;
            device.AccessToken = payload.AccessToken;
            device.Protocol = payload.Protocol;
            device.DataPointRetentionDays = payload.DataPointRetentionDays;
            device.CurrentFirmwareVersion = payload.CurrentFirmwareVersion;
            device.SampleRateSeconds = payload.SampleRateSeconds;
            device.DeviceTemplateId = payload.DeviceTemplateId;
            device.SyncedFromEdge = true;
            device.SyncedFromEdgeNodeId = edgeNode.Id;
        }

        var devicesToDelete = isFullSnapshot
            ? syncedDevicesFromEdge.Values.Where(device => !incomingDeviceIds.Contains(device.Id)).ToList()
            : syncedDevicesFromEdge.Values.Where(device => deletedDeviceIds.Contains(device.Id)).ToList();
        if (devicesToDelete.Count > 0)
        {
            summary.DevicesDeleted += devicesToDelete.Count;
            appContext.Devices.RemoveRange(devicesToDelete);
        }

        await appContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await SetAppliedMetadataVersionAsync(edgeNode.Id, incomingVersion);

        return summary.ToResponse(incomingVersion);
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

    private static int NormalizeVersion(int version)
    {
        return version < 0 ? 0 : version;
    }

    private async Task<int?> GetAppliedMetadataVersionAsync(Guid edgeNodeId)
    {
        var value = await redis.Db.StringGetAsync(GetMetadataVersionKey(edgeNodeId));
        if (!value.HasValue)
        {
            return null;
        }

        return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedVersion) && parsedVersion >= 0
            ? parsedVersion
            : null;
    }

    private async Task SetAppliedMetadataVersionAsync(Guid edgeNodeId, int version)
    {
        await redis.Db.StringSetAsync(
            GetMetadataVersionKey(edgeNodeId),
            version.ToString(CultureInfo.InvariantCulture),
            TimeSpan.FromDays(MetadataVersionTtlDays)
        );
    }

    private static string GetMetadataVersionKey(Guid edgeNodeId)
    {
        return $"edge-node:{edgeNodeId}:metadata-version";
    }

    private async Task<Dictionary<string, ApplicationUser>> BuildOwnerLookupAsync(
        SyncMetadataRequest request,
        CancellationToken cancellationToken
    )
    {
        var ownerEmails = request
            .Templates.Select(template => template.OwnerEmail)
            .Concat(request.Devices.Select(device => device.OwnerEmail))
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email!.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (ownerEmails.Count == 0)
        {
            return new Dictionary<string, ApplicationUser>();
        }

        var owners = await appContext
            .Users.Where(user => user.Email != null && ownerEmails.Contains(user.Email.ToLower()))
            .ToListAsync(cancellationToken);

        return owners
            .Where(owner => owner.Email != null)
            .GroupBy(owner => owner.Email!.Trim().ToLowerInvariant())
            .ToDictionary(group => group.Key, group => group.First());
    }

    private static ApplicationUser? ResolveOwner(string? ownerEmail, IReadOnlyDictionary<string, ApplicationUser> ownerLookup)
    {
        if (string.IsNullOrWhiteSpace(ownerEmail))
        {
            return null;
        }

        return ownerLookup.TryGetValue(ownerEmail.Trim().ToLowerInvariant(), out var owner) ? owner : null;
    }

    private sealed class MetadataSummaryAccumulator
    {
        public int TemplatesCreated { get; set; }
        public int TemplatesUpdated { get; set; }
        public int TemplatesDeleted { get; set; }
        public int TemplatesSkipped { get; set; }
        public int TemplatesConflicts { get; set; }
        public int DevicesCreated { get; set; }
        public int DevicesUpdated { get; set; }
        public int DevicesDeleted { get; set; }
        public int DevicesSkipped { get; set; }
        public int DevicesConflicts { get; set; }
        public int SensorsUpserted { get; set; }
        public int SensorsDeleted { get; set; }
        public int CommandsUpserted { get; set; }
        public int CommandsDeleted { get; set; }
        public int RecipesUpserted { get; set; }
        public int RecipesDeleted { get; set; }
        public int RecipeStepsUpserted { get; set; }
        public int RecipeStepsDeleted { get; set; }
        public int ControlsUpserted { get; set; }
        public int ControlsDeleted { get; set; }
        public int SkippedOwners { get; set; }
        public int MissingTemplateReferences { get; set; }

        public SyncMetadataResponse ToResponse(int version)
        {
            return new SyncMetadataResponse
            {
                TemplatesCreated = TemplatesCreated,
                TemplatesUpdated = TemplatesUpdated,
                TemplatesDeleted = TemplatesDeleted,
                TemplatesSkipped = TemplatesSkipped,
                TemplatesConflicts = TemplatesConflicts,
                DevicesCreated = DevicesCreated,
                DevicesUpdated = DevicesUpdated,
                DevicesDeleted = DevicesDeleted,
                DevicesSkipped = DevicesSkipped,
                DevicesConflicts = DevicesConflicts,
                SensorsUpserted = SensorsUpserted,
                SensorsDeleted = SensorsDeleted,
                CommandsUpserted = CommandsUpserted,
                CommandsDeleted = CommandsDeleted,
                RecipesUpserted = RecipesUpserted,
                RecipesDeleted = RecipesDeleted,
                RecipeStepsUpserted = RecipeStepsUpserted,
                RecipeStepsDeleted = RecipeStepsDeleted,
                ControlsUpserted = ControlsUpserted,
                ControlsDeleted = ControlsDeleted,
                SkippedOwners = SkippedOwners,
                MissingTemplateReferences = MissingTemplateReferences,
                AppliedEdgeMetadataVersion = version
            };
        }
    }
}
