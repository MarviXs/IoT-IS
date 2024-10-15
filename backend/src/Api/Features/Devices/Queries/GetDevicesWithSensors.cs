using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Devices.Queries;

public static class GetDevicesWithSensors
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/with-sensors",
                    async Task<Ok<List<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetDevicesWithSensors))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get devices with sensors";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<List<Response>>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var deviceParameters = message.Parameters;

            var query = context
                .Devices.AsNoTracking()
                .Where(d => d.OwnerId == message.User.GetUserId())
                .Where(d => d.Name.ToLower().Contains(StringUtils.Normalized(deviceParameters.SearchTerm)))
                .Include(d => d.DeviceTemplate)
                .ThenInclude(dt => dt.Sensors)
                .Sort(deviceParameters.SortBy ?? nameof(Device.UpdatedAt), deviceParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var devices = await query.Paginate(deviceParameters).ToListAsync(cancellationToken);

            var responseDevices = devices
                .Select(d => new Response(
                    d.Id,
                    d.Name,
                    d.DeviceTemplate?.Sensors.Select(s => new Sensor(s.Name, s.Tag, s.Unit ?? "")).ToList() ?? []
                ))
                .ToList();

            return responseDevices;
        }
    }

    public record Sensor(string Name, string Tag, string Unit);
    public record Response(Guid Id, string Name, List<Sensor> Sensors);
}
