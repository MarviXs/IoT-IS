using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Services;

public sealed class DeviceAccessTokenResolver(
    AppDbContext appContext,
    RedisService redis,
    RecentDeviceIdCache recentDeviceIdCache
)
{
    public async Task<Guid?> ResolveDeviceIdAsync(string deviceAccessToken, CancellationToken cancellationToken)
    {
        if (recentDeviceIdCache.TryGet(deviceAccessToken, out var deviceId))
        {
            return deviceId;
        }

        var redisKey = GetRedisKey(deviceAccessToken);
        var cachedDeviceId = await redis.Db.StringGetAsync(redisKey);

        if (cachedDeviceId.HasValue)
        {
            if (Guid.TryParse(cachedDeviceId!, out deviceId))
            {
                recentDeviceIdCache.Set(deviceAccessToken, deviceId);
                return deviceId;
            }

            await InvalidateAsync(deviceAccessToken);
        }

        deviceId = await appContext
            .Devices
            .AsNoTracking()
            .Where(d => d.AccessToken == deviceAccessToken)
            .Select(d => d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (deviceId == Guid.Empty)
        {
            return null;
        }

        await SetAsync(deviceAccessToken, deviceId);
        return deviceId;
    }

    public async Task SetAsync(string deviceAccessToken, Guid deviceId)
    {
        recentDeviceIdCache.Set(deviceAccessToken, deviceId);
        await redis.Db.StringSetAsync(GetRedisKey(deviceAccessToken), deviceId.ToString(), TimeSpan.FromHours(1));
    }

    public async Task InvalidateAsync(string deviceAccessToken)
    {
        recentDeviceIdCache.Remove(deviceAccessToken);
        await redis.Db.KeyDeleteAsync(GetRedisKey(deviceAccessToken));
    }

    private static string GetRedisKey(string deviceAccessToken) => $"device:{deviceAccessToken}:id";
}
