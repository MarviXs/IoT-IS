using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Services.DeviceFirmwares;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Services;

public sealed record EdgeSnapshotSyncSummary(
    int DevicesCreated,
    int DevicesUpdated,
    int DevicesDeleted,
    int TemplatesCreated,
    int TemplatesUpdated,
    int TemplatesDeleted,
    int SkippedOwnerNotFound,
    int FirmwareFilesDownloaded,
    int UnresolvedTemplateReferences
);

public class EdgeHubSnapshotSyncService(
    AppDbContext context,
    HubApiClientFactory clientFactory,
    IDeviceFirmwareFileService firmwareFileService
)
{
    private static readonly SemaphoreSlim SyncLock = new(1, 1);

    public async Task<EdgeSnapshotSyncSummary> SyncFromHubAsync(CancellationToken cancellationToken)
    {
        await SyncLock.WaitAsync(cancellationToken);
        try
        {
            var settings = await context.SystemNodeSettings.OrderBy(setting => setting.CreatedAt).FirstOrDefaultAsync(cancellationToken);
            if (settings == null || settings.NodeType != SystemNodeType.Edge)
            {
                throw new InvalidOperationException("Node is not configured as edge.");
            }

            if (string.IsNullOrWhiteSpace(settings.HubUrl) || string.IsNullOrWhiteSpace(settings.HubToken))
            {
                throw new InvalidOperationException("Hub URL or token is not configured.");
            }

            var client = clientFactory.Create(settings.HubUrl, settings.HubToken);
            using var snapshotResponse = await client.GetAsync("system/hub-sync/snapshot", cancellationToken);

            if (snapshotResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException("Hub rejected edge token.");
            }

            snapshotResponse.EnsureSuccessStatusCode();
            var snapshot = await snapshotResponse.Content.ReadFromJsonAsync<GetHubSnapshotResponse>(cancellationToken: cancellationToken);
            if (snapshot == null)
            {
                throw new InvalidOperationException("Hub snapshot response is empty.");
            }

            return await ApplySnapshotAsync(snapshot, client, cancellationToken);
        }
        finally
        {
            SyncLock.Release();
        }
    }

    private async Task<EdgeSnapshotSyncSummary> ApplySnapshotAsync(
        GetHubSnapshotResponse snapshot,
        HttpClient client,
        CancellationToken cancellationToken
    )
    {
        var usersByEmail = await context
            .Users.AsNoTracking()
            .Where(user => user.Email != null)
            .ToDictionaryAsync(user => user.Email!.Trim().ToLowerInvariant(), user => user.Id, cancellationToken);

        var templateIdsFromHub = snapshot.Templates.Select(template => ParseGuid(template.Id)).Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();

        var templatesDeleted = await DeleteStaleSyncedTemplatesAsync(templateIdsFromHub, cancellationToken);

        var templatesCreated = 0;
        var templatesUpdated = 0;
        var skippedOwnerNotFound = 0;
        var firmwareFilesDownloaded = 0;

        var firmwareToDownload = new List<(Guid TemplateId, Guid FirmwareId, string StoredFileName)>();

        foreach (var remoteTemplate in snapshot.Templates)
        {
            var templateId = ParseGuid(remoteTemplate.Id);
            if (!templateId.HasValue)
            {
                continue;
            }

            var ownerEmail = remoteTemplate.OwnerEmail.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ownerEmail) || !usersByEmail.TryGetValue(ownerEmail, out var ownerId))
            {
                skippedOwnerNotFound++;
                continue;
            }

            var localTemplate = await context.DeviceTemplates.FirstOrDefaultAsync(template => template.Id == templateId.Value, cancellationToken);
            if (localTemplate == null)
            {
                localTemplate = new DeviceTemplate { Id = templateId.Value, Name = remoteTemplate.Name, OwnerId = ownerId };
                await context.DeviceTemplates.AddAsync(localTemplate, cancellationToken);
                templatesCreated++;
            }
            else
            {
                templatesUpdated++;
            }

            localTemplate.OwnerId = ownerId;
            localTemplate.Name = remoteTemplate.Name;
            localTemplate.DeviceType = (DeviceType)remoteTemplate.DeviceType;
            localTemplate.IsGlobal = remoteTemplate.IsGlobal;
            localTemplate.EnableMap = remoteTemplate.EnableMap;
            localTemplate.EnableGrid = remoteTemplate.EnableGrid;
            localTemplate.GridRowSpan = remoteTemplate.HasGridRowSpan ? remoteTemplate.GridRowSpan : null;
            localTemplate.GridColumnSpan = remoteTemplate.HasGridColumnSpan ? remoteTemplate.GridColumnSpan : null;
            localTemplate.IsSyncedFromHub = true;

            await ReplaceTemplateChildrenAsync(remoteTemplate, templateId.Value, firmwareToDownload, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var firmwareDownload in firmwareToDownload)
        {
            try
            {
                var path = $"system/hub-sync/firmwares/{firmwareDownload.TemplateId}/{firmwareDownload.FirmwareId}";
                using var downloadResponse = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (downloadResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }

                downloadResponse.EnsureSuccessStatusCode();
                await using var contentStream = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
                await firmwareFileService.SaveStreamAsAsync(contentStream, firmwareDownload.StoredFileName, cancellationToken);
                firmwareFilesDownloaded++;
            }
            catch (HttpRequestException)
            {
                // Keep metadata synced even if file transfer failed for a subset.
            }
        }

        return new EdgeSnapshotSyncSummary(
            0,
            0,
            0,
            templatesCreated,
            templatesUpdated,
            templatesDeleted,
            skippedOwnerNotFound,
            firmwareFilesDownloaded,
            0
        );
    }

    private async Task<int> DeleteStaleSyncedTemplatesAsync(HashSet<Guid> templateIdsFromHub, CancellationToken cancellationToken)
    {
        var staleTemplates = await context
            .DeviceTemplates.Include(template => template.Firmwares)
            .Where(template => template.IsSyncedFromHub && !templateIdsFromHub.Contains(template.Id))
            .ToListAsync(cancellationToken);

        if (staleTemplates.Count == 0)
        {
            return 0;
        }

        foreach (var firmware in staleTemplates.SelectMany(template => template.Firmwares))
        {
            firmwareFileService.Delete(firmware.StoredFileName);
        }

        context.DeviceTemplates.RemoveRange(staleTemplates);
        await context.SaveChangesAsync(cancellationToken);
        return staleTemplates.Count;
    }

    private async Task ReplaceTemplateChildrenAsync(
        HubTemplate remoteTemplate,
        Guid templateId,
        List<(Guid TemplateId, Guid FirmwareId, string StoredFileName)> firmwareToDownload,
        CancellationToken cancellationToken
    )
    {
        var existingControls = await context.DeviceControls.Where(control => control.DeviceTemplateId == templateId).ToListAsync(cancellationToken);
        if (existingControls.Count != 0)
        {
            context.DeviceControls.RemoveRange(existingControls);
        }

        var existingRecipeIds = await context.Recipes.Where(recipe => recipe.DeviceTemplateId == templateId).Select(recipe => recipe.Id).ToListAsync(cancellationToken);
        if (existingRecipeIds.Count != 0)
        {
            var existingSteps = await context.RecipeSteps.Where(step => existingRecipeIds.Contains(step.RecipeId)).ToListAsync(cancellationToken);
            if (existingSteps.Count != 0)
            {
                context.RecipeSteps.RemoveRange(existingSteps);
            }
        }

        var existingCommands = await context.Commands.Where(command => command.DeviceTemplateId == templateId).ToListAsync(cancellationToken);
        if (existingCommands.Count != 0)
        {
            context.Commands.RemoveRange(existingCommands);
        }

        var existingRecipes = await context.Recipes.Where(recipe => recipe.DeviceTemplateId == templateId).ToListAsync(cancellationToken);
        if (existingRecipes.Count != 0)
        {
            context.Recipes.RemoveRange(existingRecipes);
        }

        var existingSensors = await context.Sensors.Where(sensor => sensor.DeviceTemplateId == templateId).ToListAsync(cancellationToken);
        if (existingSensors.Count != 0)
        {
            context.Sensors.RemoveRange(existingSensors);
        }

        var existingFirmwares = await context.DeviceFirmwares.Where(firmware => firmware.DeviceTemplateId == templateId).ToListAsync(cancellationToken);
        if (existingFirmwares.Count != 0)
        {
            foreach (var firmware in existingFirmwares)
            {
                firmwareFileService.Delete(firmware.StoredFileName);
            }

            context.DeviceFirmwares.RemoveRange(existingFirmwares);
        }

        foreach (var remoteSensor in remoteTemplate.Sensors)
        {
            var sensorId = ParseGuid(remoteSensor.Id) ?? Guid.NewGuid();
            await context.Sensors.AddAsync(
                new Sensor
                {
                    Id = sensorId,
                    DeviceTemplateId = templateId,
                    Tag = remoteSensor.Tag,
                    Name = remoteSensor.Name,
                    Unit = remoteSensor.HasUnit ? remoteSensor.Unit : null,
                    AccuracyDecimals = remoteSensor.HasAccuracyDecimals ? remoteSensor.AccuracyDecimals : null,
                    Order = remoteSensor.Order,
                    Group = remoteSensor.HasGroup ? remoteSensor.Group : null
                },
                cancellationToken
            );
        }

        foreach (var remoteCommand in remoteTemplate.Commands)
        {
            var commandId = ParseGuid(remoteCommand.Id) ?? Guid.NewGuid();
            await context.Commands.AddAsync(
                new Command
                {
                    Id = commandId,
                    DeviceTemplateId = templateId,
                    DisplayName = remoteCommand.DisplayName,
                    Name = remoteCommand.Name,
                    Params = [.. remoteCommand.Params]
                },
                cancellationToken
            );
        }

        foreach (var remoteRecipe in remoteTemplate.Recipes)
        {
            var recipeId = ParseGuid(remoteRecipe.Id) ?? Guid.NewGuid();
            await context.Recipes.AddAsync(new Recipe { Id = recipeId, DeviceTemplateId = templateId, Name = remoteRecipe.Name }, cancellationToken);
        }

        foreach (var remoteRecipe in remoteTemplate.Recipes)
        {
            var recipeId = ParseGuid(remoteRecipe.Id);
            if (!recipeId.HasValue)
            {
                continue;
            }

            foreach (var remoteStep in remoteRecipe.Steps)
            {
                await context.RecipeSteps.AddAsync(
                    new RecipeStep
                    {
                        Id = ParseGuid(remoteStep.Id) ?? Guid.NewGuid(),
                        RecipeId = recipeId.Value,
                        CommandId = remoteStep.HasCommandId ? ParseGuid(remoteStep.CommandId!) : null,
                        SubrecipeId = remoteStep.HasSubrecipeId ? ParseGuid(remoteStep.SubrecipeId!) : null,
                        Cycles = remoteStep.Cycles,
                        Order = remoteStep.Order
                    },
                    cancellationToken
                );
            }
        }

        foreach (var remoteControl in remoteTemplate.Controls)
        {
            await context.DeviceControls.AddAsync(
                new DeviceControl
                {
                    Id = ParseGuid(remoteControl.Id) ?? Guid.NewGuid(),
                    DeviceTemplateId = templateId,
                    Name = remoteControl.Name,
                    Color = remoteControl.Color,
                    Type = (DeviceControlType)remoteControl.Type,
                    RecipeId = remoteControl.HasRecipeId ? ParseGuid(remoteControl.RecipeId!) : null,
                    Cycles = remoteControl.Cycles,
                    IsInfinite = remoteControl.IsInfinite,
                    Order = remoteControl.Order,
                    RecipeOnId = remoteControl.HasRecipeOnId ? ParseGuid(remoteControl.RecipeOnId!) : null,
                    RecipeOffId = remoteControl.HasRecipeOffId ? ParseGuid(remoteControl.RecipeOffId!) : null,
                    SensorId = remoteControl.HasSensorId ? ParseGuid(remoteControl.SensorId!) : null
                },
                cancellationToken
            );
        }

        foreach (var remoteFirmware in remoteTemplate.Firmwares)
        {
            var firmwareId = ParseGuid(remoteFirmware.Id);
            if (!firmwareId.HasValue)
            {
                continue;
            }

            await context.DeviceFirmwares.AddAsync(
                new DeviceFirmware
                {
                    Id = firmwareId.Value,
                    DeviceTemplateId = templateId,
                    VersionNumber = remoteFirmware.VersionNumber,
                    IsActive = remoteFirmware.IsActive,
                    OriginalFileName = remoteFirmware.OriginalFileName,
                    StoredFileName = remoteFirmware.StoredFileName
                },
                cancellationToken
            );

            firmwareToDownload.Add((templateId, firmwareId.Value, remoteFirmware.StoredFileName));
        }
    }

    private static Guid? ParseGuid(string value)
    {
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
