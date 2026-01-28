using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Devices.Queries;

public static class GetDevices
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetDevices))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated devices";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var deviceParameters = message.Parameters;
            var sortBy = deviceParameters.SortBy?.Trim();
            var normalizedSort = sortBy?.ToLowerInvariant();
            var sortByStatus = normalizedSort is "status" or "connectionstate";
            var sortByLastActivity = normalizedSort is "lastactivity" or "lastseen";

            var query = context
                .Devices.AsNoTracking()
                .WhereOwnedOrShared(message.User.GetUserId())
                .Where(d => d.Name.ToLower().Contains(StringUtils.Normalized(deviceParameters.SearchTerm)))
                .Sort(sortByStatus || sortByLastActivity ? null : sortBy ?? nameof(Device.UpdatedAt), deviceParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            if (!sortByStatus && !sortByLastActivity)
            {
                var devices = await query.Paginate(deviceParameters).ToListAsync(cancellationToken);

                // Prepare Redis keys for MGet
                var deviceIds = devices.Select(d => d.Id).ToList();
                var onlineKeys = deviceIds.Select(id => (RedisKey)$"device:{id}:connected").ToArray();
                var lastSeenKeys = deviceIds.Select(id => (RedisKey)$"device:{id}:lastSeen").ToArray();

                // Fetch online status and last seen from Redis
                var onlineStatuses = await redis.Db.StringGetAsync(onlineKeys);
                var lastSeenTimestamps = await redis.Db.StringGetAsync(lastSeenKeys);

                // Map Redis data to devices
                var responseDevices = devices
                    .Select(
                        (device, index) =>
                        {
                            var online = onlineStatuses[index].HasValue && onlineStatuses[index] == "1";
                            DateTimeOffset? lastSeen = null;

                            if (lastSeenTimestamps[index].HasValue && long.TryParse(lastSeenTimestamps[index], out var timestamp))
                            {
                                lastSeen = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                            }
                            var connectionState = device.GetConnectionState(online, lastSeen, DateTimeOffset.UtcNow);
                            return new Response(device.Id, device.Name, online, connectionState, device.GetPermission(message.User), lastSeen);
                        }
                    )
                    .ToList();

                return responseDevices.ToPagedList(totalCount, deviceParameters.PageNumber, deviceParameters.PageSize);
            }

            var deviceSnapshots = await query
                .Select(device => new
                {
                    device.Id,
                    device.Protocol,
                    device.SampleRateSeconds
                })
                .ToListAsync(cancellationToken);

            if (deviceSnapshots.Count == 0)
            {
                return new List<Response>().ToPagedList(totalCount, deviceParameters.PageNumber, deviceParameters.PageSize);
            }

            var deviceIdsForSort = deviceSnapshots.Select(device => device.Id).ToList();
            var sortOnlineKeys = deviceIdsForSort.Select(id => (RedisKey)$"device:{id}:connected").ToArray();
            var sortLastSeenKeys = deviceIdsForSort.Select(id => (RedisKey)$"device:{id}:lastSeen").ToArray();

            var sortOnlineStatuses = await redis.Db.StringGetAsync(sortOnlineKeys);
            var sortLastSeenTimestamps = await redis.Db.StringGetAsync(sortLastSeenKeys);

            var nowUtc = DateTimeOffset.UtcNow;
            var deviceStateById = new Dictionary<Guid, (bool Online, DateTimeOffset? LastSeen, DeviceConnectionState State)>(deviceSnapshots.Count);

            for (var index = 0; index < deviceSnapshots.Count; index++)
            {
                var snapshot = deviceSnapshots[index];
                var online = sortOnlineStatuses[index].HasValue && sortOnlineStatuses[index] == "1";
                DateTimeOffset? lastSeen = null;

                if (sortLastSeenTimestamps[index].HasValue && long.TryParse(sortLastSeenTimestamps[index], out var timestamp))
                {
                    lastSeen = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                }

                var connectionState = DeviceExtensions.GetConnectionState(snapshot.Protocol, snapshot.SampleRateSeconds, online, lastSeen, nowUtc);

                deviceStateById[snapshot.Id] = (online, lastSeen, connectionState);
            }

            IEnumerable<Guid> orderedIds = deviceSnapshots.Select(snapshot => snapshot.Id);
            if (sortByStatus)
            {
                var stateItems = deviceSnapshots.Select(snapshot =>
                {
                    var state = deviceStateById[snapshot.Id].State;
                    return new { snapshot.Id, StateOrder = (int)state };
                });
                orderedIds =
                    (deviceParameters.Descending ?? false)
                        ? stateItems.OrderByDescending(item => item.StateOrder).ThenBy(item => item.Id).Select(item => item.Id)
                        : stateItems.OrderBy(item => item.StateOrder).ThenBy(item => item.Id).Select(item => item.Id);
            }
            else if (sortByLastActivity)
            {
                var lastSeenItems = deviceSnapshots.Select(snapshot =>
                {
                    var lastSeen = deviceStateById[snapshot.Id].LastSeen;
                    return new
                    {
                        snapshot.Id,
                        HasLastSeen = lastSeen.HasValue,
                        LastSeen = lastSeen ?? DateTimeOffset.MinValue
                    };
                });
                orderedIds =
                    (deviceParameters.Descending ?? false)
                        ? lastSeenItems
                            .OrderByDescending(item => item.HasLastSeen)
                            .ThenBy(item => item.LastSeen)
                            .ThenBy(item => item.Id)
                            .Select(item => item.Id)
                        : lastSeenItems
                            .OrderByDescending(item => item.HasLastSeen)
                            .ThenByDescending(item => item.LastSeen)
                            .ThenBy(item => item.Id)
                            .Select(item => item.Id);
            }

            var pageNumber = deviceParameters.PageNumber ?? 1;
            var pageSize = deviceParameters.PageSize.GetValueOrDefault();
            var pagedIds = orderedIds.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            if (pagedIds.Count == 0)
            {
                return new List<Response>().ToPagedList(totalCount, deviceParameters.PageNumber, deviceParameters.PageSize);
            }

            var devicesForPage = await query.Where(device => pagedIds.Contains(device.Id)).ToListAsync(cancellationToken);
            var deviceById = devicesForPage.ToDictionary(device => device.Id);

            var responseDevicesForPage = pagedIds
                .Select(id =>
                {
                    var device = deviceById[id];
                    var state = deviceStateById[id];
                    return new Response(device.Id, device.Name, state.Online, state.State, device.GetPermission(message.User), state.LastSeen);
                })
                .ToList();

            return responseDevicesForPage.ToPagedList(totalCount, deviceParameters.PageNumber, deviceParameters.PageSize);
        }
    }

    public record Response(
        Guid Id,
        string Name,
        bool Connected,
        DeviceConnectionState ConnectionState,
        DevicePermission Permission,
        DateTimeOffset? LastSeen = null
    );
}
