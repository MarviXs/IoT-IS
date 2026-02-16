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
                    setting.HubToken
                })
                .FirstOrDefaultAsync(cancellationToken);

            var edgeNodeSnapshots = await context
                .EdgeNodes.AsNoTracking()
                .OrderBy(edgeNode => edgeNode.Name)
                .ToListAsync(cancellationToken);

            var edgeNodeStatusById = new Dictionary<Guid, (bool IsOnline, DateTimeOffset? LastSyncAt)>(edgeNodeSnapshots.Count);
            if (edgeNodeSnapshots.Count > 0)
            {
                var keys = edgeNodeSnapshots.Select(edgeNode => (RedisKey)$"edge-node:{edgeNode.Id}:last-sync").ToArray();
                var values = await redis.Db.StringGetAsync(keys);
                var now = DateTimeOffset.UtcNow;

                for (var index = 0; index < edgeNodeSnapshots.Count; index++)
                {
                    var snapshot = edgeNodeSnapshots[index];
                    DateTimeOffset? lastSyncAt = null;
                    if (values[index].HasValue && long.TryParse(values[index], out var unixSeconds))
                    {
                        lastSyncAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                    }

                    var thresholdSeconds = Math.Max(1, (int)Math.Ceiling(snapshot.UpdateRateSeconds * 1.5d));
                    var isOnline = lastSyncAt.HasValue && (now - lastSyncAt.Value) <= TimeSpan.FromSeconds(thresholdSeconds);
                    edgeNodeStatusById[snapshot.Id] = (isOnline, lastSyncAt);
                }
            }

            var edgeNodes = edgeNodeSnapshots
                .Select(snapshot =>
                {
                    var status = edgeNodeStatusById.TryGetValue(snapshot.Id, out var resolvedStatus)
                        ? resolvedStatus
                        : (IsOnline: false, LastSyncAt: (DateTimeOffset?)null);
                    return new EdgeNodeResponse(
                        snapshot.Id,
                        snapshot.Name,
                        snapshot.Token,
                        snapshot.UpdateRateSeconds,
                        snapshot.CreatedAt,
                        snapshot.UpdatedAt,
                        status.IsOnline,
                        status.LastSyncAt
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

                var thresholdSeconds = Math.Max(1, (int)Math.Ceiling((expectedSyncSeconds ?? 5) * 1.5d));
                var isOnline = lastSyncAt.HasValue && (DateTimeOffset.UtcNow - lastSyncAt.Value) <= TimeSpan.FromSeconds(thresholdSeconds);

                hubConnectionStatus = new HubConnectionStatusResponse(isOnline, lastSyncAt, expectedSyncSeconds);
            }

            return new Response(
                settings?.NodeType ?? SystemNodeType.Hub,
                settings?.HubUrl,
                settings?.HubToken,
                edgeNodes,
                hubConnectionStatus
            );
        }
    }

    public record Response(
        SystemNodeType NodeType,
        string? HubUrl,
        string? HubToken,
        IReadOnlyCollection<EdgeNodeResponse> EdgeNodes,
        HubConnectionStatusResponse? HubConnectionStatus
    );

    public record EdgeNodeResponse(
        Guid Id,
        string Name,
        string Token,
        int UpdateRateSeconds,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool IsOnline,
        DateTimeOffset? LastSyncAt
    );

    public record HubConnectionStatusResponse(bool IsOnline, DateTimeOffset? LastSyncAt, int? ExpectedSyncSeconds);
}
