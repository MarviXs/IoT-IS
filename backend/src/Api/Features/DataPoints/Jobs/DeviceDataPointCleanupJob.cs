using System.Linq;
using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fei.Is.Api.Features.DataPoints.Jobs;

[DisallowConcurrentExecution]
public class DeviceDataPointCleanupJob(
    AppDbContext appDbContext,
    TimeScaleDbContext timeScaleDbContext,
    ILogger<DeviceDataPointCleanupJob> logger
) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        var devices = await appDbContext
            .Devices.AsNoTracking()
            .Where(device => device.DataPointRetentionDays.HasValue && device.DataPointRetentionDays > 0)
            .Select(device => new { device.Id, device.DataPointRetentionDays })
            .ToListAsync(cancellationToken);

        if (devices.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var group in devices.GroupBy(device => device.DataPointRetentionDays!.Value))
        {
            var retentionDays = group.Key;
            var cutoff = now.AddDays(-retentionDays);
            var deviceIds = group.Select(device => device.Id).ToList();

            var deleted = await timeScaleDbContext
                .DataPoints
                .Where(dataPoint => deviceIds.Contains(dataPoint.DeviceId) && dataPoint.TimeStamp < cutoff)
                .ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
            {
                logger.LogInformation(
                    "Deleted {Count} datapoints older than {Retention} days for {DeviceCount} devices.",
                    deleted,
                    retentionDays,
                    deviceIds.Count
                );
            }
        }
    }
}
