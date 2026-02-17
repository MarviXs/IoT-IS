using Fei.Is.Api.Data.Enums;
using System.Text.Json.Serialization;

namespace Fei.Is.Api.Features.System.Services;

public sealed class SyncDataPointsRequest
{
    public int EdgeMetadataVersion { get; init; }
    public List<HubDataPoint> Datapoints { get; init; } = [];
}

public sealed class HubDataPoint
{
    public string DeviceId { get; init; } = string.Empty;
    public string SensorTag { get; init; } = string.Empty;
    public double Value { get; init; }
    public long TimestampUnixMs { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int? GridX { get; init; }
    public int? GridY { get; init; }

    [JsonIgnore]
    public bool HasLatitude => Latitude.HasValue;

    [JsonIgnore]
    public bool HasLongitude => Longitude.HasValue;

    [JsonIgnore]
    public bool HasGridX => GridX.HasValue;

    [JsonIgnore]
    public bool HasGridY => GridY.HasValue;
}

public sealed class SyncDataPointsResponse
{
    public int NextSyncSeconds { get; init; }
    public int AcceptedCount { get; init; }
    public int SkippedCount { get; init; }
    public bool RequiresMetadataSync { get; init; }
    public bool DatapointsProcessed { get; init; }
}

public sealed class SyncMetadataRequest
{
    public int EdgeMetadataVersion { get; init; }
    public List<SyncedTemplatePayload> Templates { get; init; } = [];
    public List<SyncedDevicePayload> Devices { get; init; } = [];
}

public sealed class SyncedTemplatePayload
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DeviceType DeviceType { get; init; } = DeviceType.Generic;
    public bool IsGlobal { get; init; }
    public bool EnableMap { get; init; }
    public bool EnableGrid { get; init; }
    public int? GridRowSpan { get; init; }
    public int? GridColumnSpan { get; init; }
    public string? OwnerEmail { get; init; }
    public List<SyncedSensorPayload> Sensors { get; init; } = [];
    public List<SyncedCommandPayload> Commands { get; init; } = [];
    public List<SyncedRecipePayload> Recipes { get; init; } = [];
    public List<SyncedControlPayload> Controls { get; init; } = [];
}

public sealed class SyncedSensorPayload
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Tag { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public int? AccuracyDecimals { get; init; }
    public int Order { get; init; }
    public string? Group { get; init; }
}

public sealed class SyncedCommandPayload
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public List<double> Params { get; init; } = [];
}

public sealed class SyncedRecipePayload
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public List<SyncedRecipeStepPayload> Steps { get; init; } = [];
}

public sealed class SyncedRecipeStepPayload
{
    public Guid Id { get; init; }
    public Guid? CommandId { get; init; }
    public Guid? SubrecipeId { get; init; }
    public int Cycles { get; init; } = 1;
    public int Order { get; init; }
}

public sealed class SyncedControlPayload
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public DeviceControlType Type { get; init; } = DeviceControlType.Run;
    public Guid? RecipeId { get; init; }
    public Guid? RecipeOnId { get; init; }
    public Guid? RecipeOffId { get; init; }
    public Guid? SensorId { get; init; }
    public int Cycles { get; init; } = 1;
    public bool IsInfinite { get; init; }
    public int Order { get; init; }
}

public sealed class SyncedDevicePayload
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Mac { get; init; }
    public string AccessToken { get; init; } = string.Empty;
    public DeviceConnectionProtocol Protocol { get; init; } = DeviceConnectionProtocol.HTTP;
    public int? DataPointRetentionDays { get; init; }
    public string? CurrentFirmwareVersion { get; init; }
    public float? SampleRateSeconds { get; init; }
    public Guid? DeviceTemplateId { get; init; }
    public string? OwnerEmail { get; init; }
}

public sealed class SyncMetadataResponse
{
    public int TemplatesCreated { get; init; }
    public int TemplatesUpdated { get; init; }
    public int TemplatesDeleted { get; init; }
    public int TemplatesSkipped { get; init; }
    public int TemplatesConflicts { get; init; }
    public int DevicesCreated { get; init; }
    public int DevicesUpdated { get; init; }
    public int DevicesDeleted { get; init; }
    public int DevicesSkipped { get; init; }
    public int DevicesConflicts { get; init; }
    public int SensorsUpserted { get; init; }
    public int SensorsDeleted { get; init; }
    public int CommandsUpserted { get; init; }
    public int CommandsDeleted { get; init; }
    public int RecipesUpserted { get; init; }
    public int RecipesDeleted { get; init; }
    public int RecipeStepsUpserted { get; init; }
    public int RecipeStepsDeleted { get; init; }
    public int ControlsUpserted { get; init; }
    public int ControlsDeleted { get; init; }
    public int SkippedOwners { get; init; }
    public int MissingTemplateReferences { get; init; }
    public int AppliedEdgeMetadataVersion { get; init; }
}
