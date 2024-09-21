namespace Fei.Is.Api.SignalR.Dtos;

public record SensorLastDataPointDto(string deviceId, string tag, double value);
