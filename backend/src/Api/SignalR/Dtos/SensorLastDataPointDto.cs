using System;

namespace Fei.Is.Api.SignalR.Dtos;

public record SensorLastDataPointDto(
    string DeviceId,
    string Tag,
    double? Value,
    double? Latitude,
    double? Longitude,
    int? GridX,
    int? GridY,
    DateTimeOffset? Ts
);
