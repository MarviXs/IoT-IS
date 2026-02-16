using System.Text.Json.Serialization;

namespace Fei.Is.Api.Features.System.Services;

public sealed class SyncDataPointsRequest
{
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
}

public sealed class GetHubSnapshotResponse
{
    public List<HubTemplate> Templates { get; init; } = [];
    public List<HubDevice> Devices { get; init; } = [];
}

public sealed class HubTemplate
{
    public string Id { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int DeviceType { get; init; }
    public bool IsGlobal { get; init; }
    public bool EnableMap { get; init; }
    public bool EnableGrid { get; init; }
    public int? GridRowSpan { get; init; }
    public int? GridColumnSpan { get; init; }
    public List<HubSensor> Sensors { get; init; } = [];
    public List<HubCommand> Commands { get; init; } = [];
    public List<HubRecipe> Recipes { get; init; } = [];
    public List<HubDeviceControl> Controls { get; init; } = [];
    public List<HubFirmware> Firmwares { get; init; } = [];

    [JsonIgnore]
    public bool HasGridRowSpan => GridRowSpan.HasValue;

    [JsonIgnore]
    public bool HasGridColumnSpan => GridColumnSpan.HasValue;
}

public sealed class HubSensor
{
    public string Id { get; init; } = string.Empty;
    public string Tag { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public int? AccuracyDecimals { get; init; }
    public int Order { get; init; }
    public string? Group { get; init; }

    [JsonIgnore]
    public bool HasUnit => !string.IsNullOrWhiteSpace(Unit);

    [JsonIgnore]
    public bool HasAccuracyDecimals => AccuracyDecimals.HasValue;

    [JsonIgnore]
    public bool HasGroup => !string.IsNullOrWhiteSpace(Group);
}

public sealed class HubCommand
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public List<double> Params { get; init; } = [];
}

public sealed class HubRecipe
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public List<HubRecipeStep> Steps { get; init; } = [];
}

public sealed class HubRecipeStep
{
    public string Id { get; init; } = string.Empty;
    public string? CommandId { get; init; }
    public string? SubrecipeId { get; init; }
    public int Cycles { get; init; }
    public int Order { get; init; }

    [JsonIgnore]
    public bool HasCommandId => !string.IsNullOrWhiteSpace(CommandId);

    [JsonIgnore]
    public bool HasSubrecipeId => !string.IsNullOrWhiteSpace(SubrecipeId);
}

public sealed class HubDeviceControl
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int Type { get; init; }
    public string? RecipeId { get; init; }
    public int Cycles { get; init; }
    public bool IsInfinite { get; init; }
    public int Order { get; init; }
    public string? RecipeOnId { get; init; }
    public string? RecipeOffId { get; init; }
    public string? SensorId { get; init; }

    [JsonIgnore]
    public bool HasRecipeId => !string.IsNullOrWhiteSpace(RecipeId);

    [JsonIgnore]
    public bool HasRecipeOnId => !string.IsNullOrWhiteSpace(RecipeOnId);

    [JsonIgnore]
    public bool HasRecipeOffId => !string.IsNullOrWhiteSpace(RecipeOffId);

    [JsonIgnore]
    public bool HasSensorId => !string.IsNullOrWhiteSpace(SensorId);
}

public sealed class HubFirmware
{
    public string Id { get; init; } = string.Empty;
    public string VersionNumber { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string StoredFileName { get; init; } = string.Empty;
}

public sealed class HubDevice
{
    public string Id { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string? TemplateId { get; init; }
    public int Protocol { get; init; }
    public int? DataPointRetentionDays { get; init; }
    public float? SampleRateSeconds { get; init; }
    public string? CurrentFirmwareVersion { get; init; }
    public string? Mac { get; init; }

    [JsonIgnore]
    public bool HasTemplateId => !string.IsNullOrWhiteSpace(TemplateId);

    [JsonIgnore]
    public bool HasDataPointRetentionDays => DataPointRetentionDays.HasValue;

    [JsonIgnore]
    public bool HasSampleRateSeconds => SampleRateSeconds.HasValue;

    [JsonIgnore]
    public bool HasCurrentFirmwareVersion => !string.IsNullOrWhiteSpace(CurrentFirmwareVersion);

    [JsonIgnore]
    public bool HasMac => !string.IsNullOrWhiteSpace(Mac);
}
