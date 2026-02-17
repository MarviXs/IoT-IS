using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Services;

public static class EdgeSyncNowRedisKeys
{
    public static RedisKey BuildForceFullSyncKey(Guid edgeNodeId) => (RedisKey)$"edge-node:{edgeNodeId}:force-full-sync";
}
