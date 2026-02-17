using System.Globalization;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.System.Services;
using Fei.Is.Api.Redis;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.BackgroundServices;

public class EdgeHubMetadataAutoSyncBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<EdgeHubMetadataAutoSyncBackgroundService> logger
) : BackgroundService
{
    private const int DefaultSyncSeconds = 30;
    private const int MinimumSyncSeconds = 5;
    private const string LastAnnouncedHubHashKey = "edge:hub:last-announced-hash";
    private const string SnapshotHashKey = "edge:hub:last-snapshot-hash";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delaySeconds = DefaultSyncSeconds;
            try
            {
                using var scope = serviceProvider.CreateScope();
                var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var redis = scope.ServiceProvider.GetRequiredService<RedisService>();
                var snapshotSyncService = scope.ServiceProvider.GetRequiredService<EdgeHubSnapshotSyncService>();

                var settings = await appContext.SystemNodeSettings.OrderBy(setting => setting.CreatedAt).FirstOrDefaultAsync(stoppingToken);
                if (
                    settings == null
                    || settings.NodeType != SystemNodeType.Edge
                    || string.IsNullOrWhiteSpace(settings.HubUrl)
                    || string.IsNullOrWhiteSpace(settings.HubToken)
                )
                {
                    await Task.Delay(TimeSpan.FromSeconds(DefaultSyncSeconds), stoppingToken);
                    continue;
                }

                delaySeconds = await ResolveDelaySecondsAsync(redis);

                var announcedHash = await redis.Db.StringGetAsync(LastAnnouncedHubHashKey);
                if (!announcedHash.HasValue)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    continue;
                }

                var hubHash = announcedHash.ToString().Trim();
                if (string.IsNullOrWhiteSpace(hubHash))
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    continue;
                }

                var localHash = await redis.Db.StringGetAsync(SnapshotHashKey);
                if (localHash.HasValue && string.Equals(localHash.ToString(), hubHash, StringComparison.Ordinal))
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                    continue;
                }

                await snapshotSyncService.SyncFromHubAsync(stoppingToken);
                await redis.Db.StringSetAsync(SnapshotHashKey, hubHash, TimeSpan.FromDays(30));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Edge metadata auto-sync failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
        }
    }

    private static async Task<int> ResolveDelaySecondsAsync(RedisService redis)
    {
        var expectedSyncValue = await redis.Db.StringGetAsync("edge:hub:expected-sync-seconds");
        if (
            expectedSyncValue.HasValue
            && int.TryParse(expectedSyncValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected)
            && expected > 0
        )
        {
            return Math.Max(MinimumSyncSeconds, expected);
        }

        return DefaultSyncSeconds;
    }
}
