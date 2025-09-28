using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DataPoints.Queries;

public static class GetLatestDataPoints
{
    public class QueryParameters
    {
        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/sensors/{sensorTag}/data/latest",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid deviceId,
                        string sensorTag,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(deviceId, sensorTag, user, parameters);
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
                .WithName(nameof(GetLatestDataPoints))
                .WithTags(nameof(DataPoint))
                .WithOpenApi(o =>
                {
                    o.Summary = "Retrieve the most recent data points for a sensor";
                    o.Description = "Fetch the latest data points for a sensor";
                    return o;
                });
        }
    }

    public record Query(Guid DeviceId, string SensorTag, ClaimsPrincipal User, QueryParameters Parameters)
        : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext appContext, TimeScaleDbContext timescaleContext, RedisService redis) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var user = message.User;
            var parameters = message.Parameters ?? new QueryParameters();

            var device = appContext.Devices.Where(d => d.Id == message.DeviceId).Include(d => d.SharedWithUsers).FirstOrDefault();
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.CanView(user))
            {
                return Result.Fail(new ForbiddenError());
            }

            var from = parameters.From;
            var to = parameters.To;
            var useCache = from == null && to == null;

            var redisKey = $"device:{message.DeviceId}:{message.SensorTag}:last";

            if (useCache)
            {
                var cachedLatestDataPoint = await redis.Db.StringGetAsync(redisKey);
                if (cachedLatestDataPoint.HasValue)
                {
                    try
                    {
                        var cachedResponse = JsonSerializer.Deserialize<Response>(cachedLatestDataPoint!);
                        if (cachedResponse != null)
                        {
                            return Result.Ok(cachedResponse);
                        }
                    }
                    catch (JsonException)
                    {
                        if (double.TryParse(cachedLatestDataPoint.ToString(), out double value))
                        {
                            return Result.Ok(new Response(null, value, null, null, null, null));
                        }
                    }
                }
            }

            var query = timescaleContext
                .DataPoints.Where(dp => dp.DeviceId == message.DeviceId && dp.SensorTag == message.SensorTag);

            if (from.HasValue)
            {
                var fromValue = from.Value;
                query = query.Where(dp => dp.TimeStamp >= fromValue);
            }

            if (to.HasValue)
            {
                var toValue = to.Value;
                query = query.Where(dp => dp.TimeStamp <= toValue);
            }

            var latestDataPoint = await query
                .OrderByDescending(dp => dp.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (latestDataPoint == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var response = new Response(
                latestDataPoint.TimeStamp,
                latestDataPoint.Value,
                latestDataPoint.Latitude,
                latestDataPoint.Longitude,
                latestDataPoint.GridX,
                latestDataPoint.GridY
            );

            if (useCache)
            {
                await redis.Db.StringSetAsync(
                    redisKey,
                    JsonSerializer.Serialize(response),
                    TimeSpan.FromHours(1)
                );
            }

            return Result.Ok(response);
        }
    }

    public record Response(DateTimeOffset? Ts, double? Value, double? Latitude, double? Longitude, int? GridX, int? GridY);
}
