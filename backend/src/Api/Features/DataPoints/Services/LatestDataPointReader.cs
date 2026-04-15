using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DataPoints.Services;

public static class LatestDataPointReader
{
    private static readonly TimeSpan[] SearchWindows =
    [
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(7),
        TimeSpan.FromDays(30),
        TimeSpan.FromDays(180),
        TimeSpan.FromDays(365)
    ];

    public static async Task<DataPoint?> GetLatestAsync(
        TimeScaleDbContext timescaleContext,
        Guid deviceId,
        string sensorTag,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken cancellationToken
    )
    {
        var query = timescaleContext
            .DataPoints.AsNoTracking()
            .Where(dp => dp.DeviceId == deviceId && dp.SensorTag == sensorTag);

        if (from.HasValue)
        {
            var fromValue = from.Value;
            query = query.Where(dp => dp.TimeStamp >= fromValue);
        }

        if (to.HasValue)
        {
            var toValue = to.Value;
            query = query.Where(dp => dp.TimeStamp <= toValue);
        }

        if (from.HasValue || to.HasValue)
        {
            return await query.OrderByDescending(dp => dp.TimeStamp).FirstOrDefaultAsync(cancellationToken);
        }

        var upperBound = DateTimeOffset.UtcNow;

        foreach (var window in SearchWindows)
        {
            var lowerBound = upperBound - window;
            var latest = await query
                .Where(dp => dp.TimeStamp >= lowerBound && dp.TimeStamp <= upperBound)
                .OrderByDescending(dp => dp.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);

            if (latest != null)
            {
                return latest;
            }
        }

        for (var i = 0; i < 24; i++)
        {
            var lowerBound = upperBound.AddYears(-1);
            var latest = await query
                .Where(dp => dp.TimeStamp >= lowerBound && dp.TimeStamp < upperBound)
                .OrderByDescending(dp => dp.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);

            if (latest != null)
            {
                return latest;
            }

            upperBound = lowerBound;
        }

        return null;
    }
}
