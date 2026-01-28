using System.Linq;
using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;

namespace Fei.Is.Api.Features.DataPoints.Jobs;

[DisallowConcurrentExecution]
public class DeviceDataPointCleanupJob(AppDbContext appDbContext, TimeScaleDbContext timeScaleDbContext, ILogger<DeviceDataPointCleanupJob> logger)
    : IJob
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

        const int deviceBatchSize = 500;
        const int deleteBatchSize = 10_000;

        foreach (var group in devices.GroupBy(device => device.DataPointRetentionDays!.Value))
        {
            var retentionDays = group.Key;
            var cutoff = now.AddDays(-retentionDays);
            var deviceIds = group.Select(device => device.Id).ToList();
            var totalDeleted = 0;

            foreach (var deviceBatch in deviceIds.Chunk(deviceBatchSize))
            {
                while (true)
                {
                    var deleted = await DeleteBatchAsync(timeScaleDbContext, deviceBatch, cutoff, deleteBatchSize, cancellationToken);

                    if (deleted == 0)
                    {
                        break;
                    }

                    totalDeleted += deleted;
                }
            }

            if (totalDeleted > 0)
            {
                logger.LogInformation(
                    "Deleted {Count} datapoints older than {Retention} days for {DeviceCount} devices.",
                    totalDeleted,
                    retentionDays,
                    deviceIds.Count
                );
            }
        }
    }

    private static Task<int> DeleteBatchAsync(
        TimeScaleDbContext timeScaleDbContext,
        IReadOnlyCollection<Guid> deviceIds,
        DateTimeOffset cutoff,
        int batchSize,
        CancellationToken cancellationToken
    )
    {
        // Limit delete size to avoid long-running commands/timeouts.
        const string sql = """
            WITH candidates AS (
                SELECT "DeviceId", "SensorTag", "TimeStamp"
                FROM "DataPoints"
                WHERE "DeviceId" = ANY (@device_ids)
                  AND "TimeStamp" < @cutoff
                LIMIT @batch_size
            )
            DELETE FROM "DataPoints" AS d
            USING candidates
            WHERE d."DeviceId" = candidates."DeviceId"
              AND d."SensorTag" = candidates."SensorTag"
              AND d."TimeStamp" = candidates."TimeStamp";
            """;

        var parameters = new[]
        {
            new NpgsqlParameter("device_ids", deviceIds.ToArray()),
            new NpgsqlParameter("cutoff", cutoff),
            new NpgsqlParameter("batch_size", batchSize)
        };

        return timeScaleDbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }
}
