using Carter;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Queries;

public static class GetNodeSettings
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "system/node-settings",
                    async Task<Ok<Response>> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Query(), cancellationToken);
                        return TypedResults.Ok(result);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(GetNodeSettings))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Get system node settings";
                    o.Description = "Returns current node type, hub connection settings, and configured edge nodes.";
                    return o;
                });
        }
    }

    public record Query() : IRequest<Response>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var settings = await context
                .SystemNodeSettings.AsNoTracking()
                .OrderBy(setting => setting.CreatedAt)
                .Select(setting => new
                {
                    setting.NodeType,
                    setting.HubUrl,
                    setting.HubToken,
                    setting.SyncIntervalSeconds,
                    setting.DataPointSyncMode
                })
                .FirstOrDefaultAsync(cancellationToken);
            var configuredSyncIntervalSeconds = settings?.SyncIntervalSeconds > 0 ? settings.SyncIntervalSeconds : 5;

            var edgeNodeSnapshots = await context
                .EdgeNodes.AsNoTracking()
                .OrderBy(edgeNode => edgeNode.Name)
                .ToListAsync(cancellationToken);

            var edgeNodeStatusById = new Dictionary<Guid, (bool IsOnline, DateTimeOffset? LastSyncAt, int? ExpectedSyncSeconds)>(edgeNodeSnapshots.Count);
            if (edgeNodeSnapshots.Count > 0)
            {
                var lastSyncKeys = edgeNodeSnapshots.Select(edgeNode => (RedisKey)$"edge-node:{edgeNode.Id}:last-sync").ToArray();
                var expectedSyncKeys = edgeNodeSnapshots.Select(edgeNode => (RedisKey)$"edge-node:{edgeNode.Id}:expected-sync-seconds").ToArray();
                var lastSyncValues = await redis.Db.StringGetAsync(lastSyncKeys);
                var expectedSyncValues = await redis.Db.StringGetAsync(expectedSyncKeys);
                var now = DateTimeOffset.UtcNow;

                for (var index = 0; index < edgeNodeSnapshots.Count; index++)
                {
                    var snapshot = edgeNodeSnapshots[index];
                    DateTimeOffset? lastSyncAt = null;
                    if (lastSyncValues[index].HasValue && long.TryParse(lastSyncValues[index], out var unixSeconds))
                    {
                        lastSyncAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                    }

                    int? expectedSyncSeconds = null;
                    if (expectedSyncValues[index].HasValue && int.TryParse(expectedSyncValues[index], out var expected))
                    {
                        expectedSyncSeconds = expected;
                    }

                    var thresholdSeconds = Math.Max(1, (int)Math.Ceiling((expectedSyncSeconds ?? configuredSyncIntervalSeconds) * 2d));
                    var isOnline = lastSyncAt.HasValue && (now - lastSyncAt.Value) <= TimeSpan.FromSeconds(thresholdSeconds);
                    edgeNodeStatusById[snapshot.Id] = (isOnline, lastSyncAt, expectedSyncSeconds);
                }
            }

            var edgeNodes = edgeNodeSnapshots
                .Select(snapshot =>
                {
                    var status = edgeNodeStatusById.TryGetValue(snapshot.Id, out var resolvedStatus)
                        ? resolvedStatus
                        : (IsOnline: false, LastSyncAt: (DateTimeOffset?)null, ExpectedSyncSeconds: (int?)null);
                    return new EdgeNodeResponse(
                        snapshot.Id,
                        snapshot.Name,
                        snapshot.Token,
                        snapshot.CreatedAt,
                        snapshot.UpdatedAt,
                        status.IsOnline,
                        status.LastSyncAt,
                        status.ExpectedSyncSeconds
                    );
                })
                .ToList();

            HubConnectionStatusResponse? hubConnectionStatus = null;
            if ((settings?.NodeType ?? SystemNodeType.Hub) == SystemNodeType.Edge)
            {
                var lastSyncValue = await redis.Db.StringGetAsync("edge:hub:last-sync");
                var expectedSyncValue = await redis.Db.StringGetAsync("edge:hub:expected-sync-seconds");

                DateTimeOffset? lastSyncAt = null;
                if (lastSyncValue.HasValue && long.TryParse(lastSyncValue, out var unixSeconds))
                {
                    lastSyncAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                }

                int? expectedSyncSeconds = null;
                if (expectedSyncValue.HasValue && int.TryParse(expectedSyncValue, out var expected))
                {
                    expectedSyncSeconds = expected;
                }

                var thresholdSeconds = Math.Max(1, (int)Math.Ceiling((expectedSyncSeconds ?? configuredSyncIntervalSeconds) * 2d));
                var isOnline = lastSyncAt.HasValue && (DateTimeOffset.UtcNow - lastSyncAt.Value) <= TimeSpan.FromSeconds(thresholdSeconds);

                hubConnectionStatus = new HubConnectionStatusResponse(isOnline, lastSyncAt);
            }

            return new Response(
                settings?.NodeType ?? SystemNodeType.Hub,
                settings?.HubUrl,
                settings?.HubToken,
                configuredSyncIntervalSeconds,
                settings?.DataPointSyncMode ?? EdgeDataPointSyncMode.OnlyNew,
                edgeNodes,
                hubConnectionStatus
            );
        }
    }

    public record Response(
        SystemNodeType NodeType,
        string? HubUrl,
        string? HubToken,
        int SyncIntervalSeconds,
        EdgeDataPointSyncMode DataPointSyncMode,
        IReadOnlyCollection<EdgeNodeResponse> EdgeNodes,
        HubConnectionStatusResponse? HubConnectionStatus
    );

    public record EdgeNodeResponse(
        Guid Id,
        string Name,
        string Token,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool IsOnline,
        DateTimeOffset? LastSyncAt,
        int? ExpectedSyncSeconds
    );

    public record HubConnectionStatusResponse(bool IsOnline, DateTimeOffset? LastSyncAt);
}
