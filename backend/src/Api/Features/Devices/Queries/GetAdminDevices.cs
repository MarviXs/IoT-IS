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

public static class GetAdminDevices
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "admin/devices",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(GetAdminDevices))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all devices for administrators";
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
            var normalizedSearch = StringUtils.Normalized(deviceParameters.SearchTerm);

            var query = context
                .Devices.AsNoTracking()
                .Include(device => device.Owner)
                .Where(
                    device =>
                        device.Name.ToLower().Contains(normalizedSearch)
                        || (device.Owner != null
                            && device.Owner.Email != null
                            && device.Owner.Email.ToLower().Contains(normalizedSearch))
                )
                .Sort(deviceParameters.SortBy ?? nameof(Device.UpdatedAt), deviceParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var devices = await query.Paginate(deviceParameters).ToListAsync(cancellationToken);

            var deviceIds = devices.Select(d => d.Id).ToList();
            var onlineKeys = deviceIds.Select(id => (RedisKey)$"device:{id}:connected").ToArray();
            var lastSeenKeys = deviceIds.Select(id => (RedisKey)$"device:{id}:lastSeen").ToArray();

            var onlineStatuses = await redis.Db.StringGetAsync(onlineKeys);
            var lastSeenTimestamps = await redis.Db.StringGetAsync(lastSeenKeys);

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
                        return new Response(
                            device.Id,
                            device.Name,
                            device.OwnerId,
                            device.Owner?.Email,
                            online,
                            connectionState,
                            device.GetPermission(message.User),
                            lastSeen
                        );
                    }
                )
                .ToList();

            return responseDevices.ToPagedList(totalCount, deviceParameters.PageNumber, deviceParameters.PageSize);
        }
    }

    public record Response(
        Guid Id,
        string Name,
        Guid OwnerId,
        string? OwnerEmail,
        bool Connected,
        DeviceConnectionState ConnectionState,
        DevicePermission Permission,
        DateTimeOffset? LastSeen = null
    );
}
