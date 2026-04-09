using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Services;

public sealed class DeviceAccessTokenResolver(AppDbContext appContext, RedisService redis)
{
    public async Task<Guid?> ResolveDeviceIdAsync(string deviceAccessToken, CancellationToken cancellationToken)
    {
        var redisKey = GetRedisKey(deviceAccessToken);
        var cachedDeviceId = await redis.Db.StringGetAsync(redisKey);

        if (cachedDeviceId.HasValue)
        {
            if (Guid.TryParse(cachedDeviceId!, out var deviceId))
            {
                return deviceId;
            }

            await InvalidateAsync(deviceAccessToken);
        }

        var resolvedDeviceId = await appContext
            .Devices.AsNoTracking()
            .Where(d => d.AccessToken == deviceAccessToken)
            .Select(d => d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (resolvedDeviceId == Guid.Empty)
        {
            return null;
        }

        await SetAsync(deviceAccessToken, resolvedDeviceId);
        return resolvedDeviceId;
    }

    public async Task SetAsync(string deviceAccessToken, Guid deviceId)
    {
        await redis.Db.StringSetAsync(GetRedisKey(deviceAccessToken), deviceId.ToString(), TimeSpan.FromHours(1));
    }

    public async Task InvalidateAsync(string deviceAccessToken)
    {
        await redis.Db.KeyDeleteAsync(GetRedisKey(deviceAccessToken));
    }

    private static string GetRedisKey(string deviceAccessToken) => $"device:{deviceAccessToken}:id";
}
