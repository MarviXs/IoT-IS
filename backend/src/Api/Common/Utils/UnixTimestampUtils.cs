namespace Fei.Is.Api.Common.Utils;

public static class UnixTimestampUtils
{
    private const int MinValidYear = 2000;
    private const long MaxLikelyUnixSeconds = 9_999_999_999;

    public static DateTimeOffset NormalizeToDateTimeOffsetOrNow(long? unixTimestamp, DateTimeOffset? now = null)
    {
        var fallback = now ?? DateTimeOffset.UtcNow;
        if (!unixTimestamp.HasValue || unixTimestamp.Value == 0)
        {
            return fallback;
        }

        if (!TryNormalizeToDateTimeOffset(unixTimestamp.Value, out var normalizedTimestamp))
        {
            return fallback;
        }

        return normalizedTimestamp.Year < MinValidYear ? fallback : normalizedTimestamp;
    }

    public static bool TryNormalizeToDateTimeOffset(long unixTimestamp, out DateTimeOffset timestamp)
    {
        try
        {
            if (IsLikelyUnixSeconds(unixTimestamp))
            {
                timestamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            }
            else
            {
                timestamp = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);
            }

            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            timestamp = default;
            return false;
        }
    }

    private static bool IsLikelyUnixSeconds(long unixTimestamp)
    {
        return unixTimestamp >= -MaxLikelyUnixSeconds && unixTimestamp <= MaxLikelyUnixSeconds;
    }
}
