using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Scenes.Services;

public sealed class SceneDeviceCacheService(RedisService redis)
{
    private const string DeviceHasEnabledScenesPrefix = "scene-device-has-enabled-scenes:";

    public async Task<bool> AnyEnabledScenesForDevicesAsync(
        AppDbContext context,
        IEnumerable<Guid> deviceIds,
        CancellationToken cancellationToken
    )
    {
        var distinctDeviceIds = deviceIds.Distinct().ToArray();
        if (distinctDeviceIds.Length == 0)
        {
            return false;
        }

        var keys = distinctDeviceIds.Select(static deviceId => (RedisKey)GetCacheKey(deviceId)).ToArray();
        var cachedValues = await redis.Db.StringGetAsync(keys);
        var missingDeviceIds = new List<Guid>();

        for (var i = 0; i < cachedValues.Length; i++)
        {
            if (cachedValues[i].HasValue)
            {
                if (cachedValues[i] == "1")
                {
                    return true;
                }

                continue;
            }

            missingDeviceIds.Add(distinctDeviceIds[i]);
        }

        if (missingDeviceIds.Count == 0)
        {
            return false;
        }

        var enabledDeviceIds = await GetEnabledSceneDeviceIdsAsync(context, missingDeviceIds, cancellationToken);
        await UpdateCacheValuesAsync(missingDeviceIds, enabledDeviceIds);

        return enabledDeviceIds.Count != 0;
    }

    public async Task RefreshDevicesAsync(AppDbContext context, IEnumerable<Guid> deviceIds, CancellationToken cancellationToken)
    {
        var distinctDeviceIds = deviceIds.Distinct().ToArray();
        if (distinctDeviceIds.Length == 0)
        {
            return;
        }

        var enabledDeviceIds = await GetEnabledSceneDeviceIdsAsync(context, distinctDeviceIds, cancellationToken);
        await UpdateCacheValuesAsync(distinctDeviceIds, enabledDeviceIds);
    }

    private async Task<HashSet<Guid>> GetEnabledSceneDeviceIdsAsync(
        AppDbContext context,
        IReadOnlyCollection<Guid> deviceIds,
        CancellationToken cancellationToken
    )
    {
        var enabledDeviceIds = await context
            .SceneSensorTriggers
            .Where(trigger => deviceIds.Contains(trigger.DeviceId) && trigger.Scene.IsEnabled)
            .Select(trigger => trigger.DeviceId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return enabledDeviceIds.ToHashSet();
    }

    private async Task UpdateCacheValuesAsync(IEnumerable<Guid> deviceIds, IReadOnlySet<Guid> enabledDeviceIds)
    {
        var batch = redis.Db.CreateBatch();
        var tasks = new List<Task>();

        foreach (var deviceId in deviceIds)
        {
            tasks.Add(batch.StringSetAsync(GetCacheKey(deviceId), enabledDeviceIds.Contains(deviceId) ? "1" : "0"));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    private static string GetCacheKey(Guid deviceId) => $"{DeviceHasEnabledScenesPrefix}{deviceId}";
}
