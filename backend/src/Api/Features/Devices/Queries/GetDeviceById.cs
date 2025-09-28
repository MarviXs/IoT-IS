using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Devices.Queries;

public static class GetDeviceById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(id, user);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetDeviceById))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a device by id";
                    return o;
                });
        }
    }

    public record Query(Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context
                .Devices.AsNoTracking()
                .Include(d => d.SharedWithUsers)
                .Include(d => d.DeviceTemplate)
                .ThenInclude(deviceTemplate => deviceTemplate!.Sensors)
                .FirstOrDefaultAsync(device => device.Id == request.DeviceId, cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.CanView(request.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            RedisKey isOnlineKey = $"device:{device.Id}:connected";
            RedisKey lastSeenKey = $"device:{device.Id}:lastSeen";

            RedisValue isOnline = await redis.Db.StringGetAsync(isOnlineKey);
            RedisValue lastSeen = await redis.Db.StringGetAsync(lastSeenKey);

            var response = new Response(
                device.Id,
                device.Name,
                device.Mac,
                device.AccessToken,
                device.DeviceTemplate != null
                    ? new TemplateResponse(
                        device.DeviceTemplate.Id,
                        device.DeviceTemplate.Name,
                        [
                            .. device
                                .DeviceTemplate.Sensors.OrderBy(sensor => sensor.Order)
                                .Select(sensor => new SensorResponse(
                                    sensor.Id,
                                    sensor.Tag,
                                    sensor.Name,
                                    sensor.Unit,
                                    sensor.AccuracyDecimals,
                                    sensor.Order,
                                    sensor.Group
                                ))
                        ],
                        device.DeviceTemplate.DeviceType,
                        device.DeviceTemplate.GridRowSpan,
                        device.DeviceTemplate.GridColumnSpan
                    )
                    : null,
                device.CreatedAt,
                device.UpdatedAt,
                isOnline.HasValue && isOnline == "1",
                lastSeen.HasValue && long.TryParse(lastSeen, out var timestamp) ? DateTimeOffset.FromUnixTimeSeconds(timestamp) : null,
                device.Protocol
            );

            return Result.Ok(response);
        }
    }

    public record SensorResponse(Guid Id, string Tag, string Name, string? Unit, int? AccuracyDecimals, int Order, string? Group);

    public record TemplateResponse(Guid Id, string Name, SensorResponse[] Sensors, DeviceType DeviceType, int? GridRowSpan = null, int? GridColumnSpan = null);

    public record Response(
        Guid Id,
        string Name,
        string? Mac,
        string? AccessToken,
        TemplateResponse? DeviceTemplate,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool Connected,
        DateTimeOffset? LastSeen,
        DeviceConnectionProtocol Protocol
    );
}
