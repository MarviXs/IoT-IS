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
