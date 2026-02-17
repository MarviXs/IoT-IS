using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Services;

public sealed class EdgeHubMetadataSnapshotService(AppDbContext appContext)
{
    public async Task<SyncMetadataRequest> BuildSnapshotAsync(int edgeMetadataVersion, CancellationToken cancellationToken)
    {
        var templates = await appContext
            .DeviceTemplates.AsNoTracking()
            .Where(template => !template.SyncedFromEdge)
            .Include(template => template.Owner)
            .Include(template => template.Sensors)
            .Include(template => template.Commands)
            .Include(template => template.Controls)
            .Include(template => template.Recipes)
            .ThenInclude(recipe => recipe.Steps)
            .OrderBy(template => template.Name)
            .ToListAsync(cancellationToken);

        var devices = await appContext
            .Devices.AsNoTracking()
            .Where(device => !device.SyncedFromEdge)
            .Include(device => device.Owner)
            .OrderBy(device => device.Name)
            .ToListAsync(cancellationToken);

        return new SyncMetadataRequest
        {
            EdgeMetadataVersion = edgeMetadataVersion,
            Templates = templates
                .Select(template => new SyncedTemplatePayload
                {
                    Id = template.Id,
                    Name = template.Name,
                    DeviceType = template.DeviceType,
                    IsGlobal = template.IsGlobal,
                    EnableMap = template.EnableMap,
                    EnableGrid = template.EnableGrid,
                    GridRowSpan = template.EnableGrid ? template.GridRowSpan : null,
                    GridColumnSpan = template.EnableGrid ? template.GridColumnSpan : null,
                    OwnerEmail = template.Owner?.Email,
                    Sensors = template
                        .Sensors.OrderBy(sensor => sensor.Order)
                        .Select(sensor => new SyncedSensorPayload
                        {
                            Id = sensor.Id,
                            Name = sensor.Name,
                            Tag = sensor.Tag,
                            Unit = sensor.Unit,
                            AccuracyDecimals = sensor.AccuracyDecimals,
                            Order = sensor.Order,
                            Group = sensor.Group
                        })
                        .ToList(),
                    Commands = template
                        .Commands.OrderBy(command => command.Name)
                        .Select(command => new SyncedCommandPayload
                        {
                            Id = command.Id,
                            DisplayName = command.DisplayName,
                            Name = command.Name,
                            Params = [.. command.Params]
                        })
                        .ToList(),
                    Recipes = template
                        .Recipes.OrderBy(recipe => recipe.Name)
                        .Select(recipe => new SyncedRecipePayload
                        {
                            Id = recipe.Id,
                            Name = recipe.Name,
                            Steps = recipe
                                .Steps.OrderBy(step => step.Order)
                                .Select(step => new SyncedRecipeStepPayload
                                {
                                    Id = step.Id,
                                    CommandId = step.CommandId,
                                    SubrecipeId = step.SubrecipeId,
                                    Cycles = step.Cycles,
                                    Order = step.Order
                                })
                                .ToList()
                        })
                        .ToList(),
                    Controls = template
                        .Controls.OrderBy(control => control.Order)
                        .Select(control => new SyncedControlPayload
                        {
                            Id = control.Id,
                            Name = control.Name,
                            Color = control.Color,
                            Type = control.Type,
                            RecipeId = control.RecipeId,
                            RecipeOnId = control.RecipeOnId,
                            RecipeOffId = control.RecipeOffId,
                            SensorId = control.SensorId,
                            Cycles = control.Cycles,
                            IsInfinite = control.IsInfinite,
                            Order = control.Order
                        })
                        .ToList()
                })
                .ToList(),
            Devices = devices
                .Select(device => new SyncedDevicePayload
                {
                    Id = device.Id,
                    Name = device.Name,
                    Mac = device.Mac,
                    AccessToken = device.AccessToken,
                    Protocol = device.Protocol,
                    DataPointRetentionDays = device.DataPointRetentionDays,
                    CurrentFirmwareVersion = device.CurrentFirmwareVersion,
                    SampleRateSeconds = device.SampleRateSeconds,
                    DeviceTemplateId = device.DeviceTemplateId,
                    OwnerEmail = device.Owner?.Email
                })
                .ToList()
        };
    }
}
