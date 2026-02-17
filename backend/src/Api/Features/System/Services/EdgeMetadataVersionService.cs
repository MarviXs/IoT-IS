using System.Globalization;
using Fei.Is.Api.Redis;

namespace Fei.Is.Api.Features.System.Services;

public sealed class EdgeMetadataVersionService(RedisService redis)
{
    private const string MetadataVersionKey = "edge:hub:metadata:version";

    public async Task<int> GetCurrentVersionAsync(CancellationToken cancellationToken = default)
    {
        var versionValue = await redis.Db.StringGetAsync(MetadataVersionKey);
        if (!versionValue.HasValue)
        {
            await redis.Db.StringSetAsync(MetadataVersionKey, "0");
            return 0;
        }

        if (int.TryParse(versionValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedVersion) && parsedVersion >= 0)
        {
            return parsedVersion;
        }

        await redis.Db.StringSetAsync(MetadataVersionKey, "0");
        return 0;
    }

    public async Task<int> IncrementVersionAsync(CancellationToken cancellationToken = default)
    {
        var nextVersion = await redis.Db.StringIncrementAsync(MetadataVersionKey);
        var normalizedVersion = nextVersion <= 0 ? 0 : nextVersion > int.MaxValue ? int.MaxValue : (int)nextVersion;
        if (normalizedVersion != nextVersion)
        {
            await redis.Db.StringSetAsync(MetadataVersionKey, normalizedVersion.ToString(CultureInfo.InvariantCulture));
        }

        return normalizedVersion;
    }
}
