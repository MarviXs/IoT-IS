using System.Globalization;
using Fei.Is.Api.Redis;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Services;

public sealed class EdgeMetadataDeltaTrackerService(RedisService redis)
{
    private const string ChangedTemplatesKey = "edge:hub:metadata:changed:templates";
    private const string DeletedTemplatesKey = "edge:hub:metadata:deleted:templates";
    private const string ChangedDevicesKey = "edge:hub:metadata:changed:devices";
    private const string DeletedDevicesKey = "edge:hub:metadata:deleted:devices";
    private const string LastSyncedVersionKey = "edge:hub:metadata:last-synced-version";

    public async Task RecordChangesAsync(EdgeMetadataChangeSet changes, CancellationToken cancellationToken = default)
    {
        if (!changes.HasChanges)
        {
            return;
        }

        if (changes.ChangedTemplateIds.Count > 0)
        {
            var values = ToRedisValues(changes.ChangedTemplateIds);
            await redis.Db.SetAddAsync(ChangedTemplatesKey, values);
            await redis.Db.SetRemoveAsync(DeletedTemplatesKey, values);
        }

        if (changes.DeletedTemplateIds.Count > 0)
        {
            var values = ToRedisValues(changes.DeletedTemplateIds);
            await redis.Db.SetAddAsync(DeletedTemplatesKey, values);
            await redis.Db.SetRemoveAsync(ChangedTemplatesKey, values);
        }

        if (changes.ChangedDeviceIds.Count > 0)
        {
            var values = ToRedisValues(changes.ChangedDeviceIds);
            await redis.Db.SetAddAsync(ChangedDevicesKey, values);
            await redis.Db.SetRemoveAsync(DeletedDevicesKey, values);
        }

        if (changes.DeletedDeviceIds.Count > 0)
        {
            var values = ToRedisValues(changes.DeletedDeviceIds);
            await redis.Db.SetAddAsync(DeletedDevicesKey, values);
            await redis.Db.SetRemoveAsync(ChangedDevicesKey, values);
        }
    }

    public async Task<EdgeMetadataChangeSet> GetPendingChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedTemplateIds = ParseIds(await redis.Db.SetMembersAsync(ChangedTemplatesKey));
        var deletedTemplateIds = ParseIds(await redis.Db.SetMembersAsync(DeletedTemplatesKey));
        var changedDeviceIds = ParseIds(await redis.Db.SetMembersAsync(ChangedDevicesKey));
        var deletedDeviceIds = ParseIds(await redis.Db.SetMembersAsync(DeletedDevicesKey));

        return new EdgeMetadataChangeSet
        {
            ChangedTemplateIds = changedTemplateIds,
            DeletedTemplateIds = deletedTemplateIds,
            ChangedDeviceIds = changedDeviceIds,
            DeletedDeviceIds = deletedDeviceIds
        };
    }

    public async Task<int?> GetLastSyncedVersionAsync(CancellationToken cancellationToken = default)
    {
        var value = await redis.Db.StringGetAsync(LastSyncedVersionKey);
        if (!value.HasValue)
        {
            return null;
        }

        return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0
            ? parsed
            : null;
    }

    public async Task MarkSyncAppliedAsync(int version, CancellationToken cancellationToken = default)
    {
        var normalizedVersion = version < 0 ? 0 : version;
        await redis.Db.StringSetAsync(LastSyncedVersionKey, normalizedVersion.ToString(CultureInfo.InvariantCulture));
        await redis.Db.KeyDeleteAsync(
            [
                (RedisKey)ChangedTemplatesKey,
                (RedisKey)DeletedTemplatesKey,
                (RedisKey)ChangedDevicesKey,
                (RedisKey)DeletedDevicesKey
            ]
        );
    }

    private static RedisValue[] ToRedisValues(IEnumerable<Guid> ids)
    {
        return ids.Select(static id => (RedisValue)id.ToString()).ToArray();
    }

    private static HashSet<Guid> ParseIds(IEnumerable<RedisValue> values)
    {
        return values.Select(value => Guid.TryParse(value.ToString(), out var id) ? id : Guid.Empty).Where(id => id != Guid.Empty).ToHashSet();
    }
}

public sealed class EdgeMetadataChangeSet
{
    public HashSet<Guid> ChangedTemplateIds { get; set; } = [];
    public HashSet<Guid> DeletedTemplateIds { get; set; } = [];
    public HashSet<Guid> ChangedDeviceIds { get; set; } = [];
    public HashSet<Guid> DeletedDeviceIds { get; set; } = [];

    public bool HasChanges =>
        ChangedTemplateIds.Count > 0 || DeletedTemplateIds.Count > 0 || ChangedDeviceIds.Count > 0 || DeletedDeviceIds.Count > 0;
}
