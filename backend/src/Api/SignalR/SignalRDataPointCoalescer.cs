using Fei.Is.Api.SignalR.Dtos;

namespace Fei.Is.Api.SignalR;

public static class SignalRDataPointCoalescer
{
    public static IReadOnlyList<SensorLastDataPointDto> CoalesceLatestByDeviceAndTag(IEnumerable<SensorLastDataPointDto> notifications)
    {
        var latestBySensor = new Dictionary<string, SensorLastDataPointDto>(StringComparer.Ordinal);

        foreach (var notification in notifications)
        {
            var key = $"{notification.DeviceId}:{notification.Tag}";

            if (!latestBySensor.TryGetValue(key, out var existing) || IsNewer(notification, existing))
            {
                latestBySensor[key] = notification;
            }
        }

        return latestBySensor.Values.ToList();
    }

    public static IReadOnlyList<SensorLastDataPointDto> CoalesceLatestByTag(
        string deviceId,
        IEnumerable<SensorLastDataPointDto> notifications
    )
    {
        var latestByTag = new Dictionary<string, SensorLastDataPointDto>(StringComparer.Ordinal);

        foreach (var notification in notifications)
        {
            var tag = notification.Tag;

            if (!latestByTag.TryGetValue(tag, out var existing) || IsNewer(notification, existing))
            {
                latestByTag[tag] = notification with { DeviceId = deviceId };
            }
        }

        return latestByTag.Values.ToList();
    }

    private static bool IsNewer(SensorLastDataPointDto candidate, SensorLastDataPointDto existing)
    {
        var candidateTimestamp = candidate.Ts?.UtcTicks ?? long.MinValue;
        var existingTimestamp = existing.Ts?.UtcTicks ?? long.MinValue;

        return candidateTimestamp >= existingTimestamp;
    }
}
