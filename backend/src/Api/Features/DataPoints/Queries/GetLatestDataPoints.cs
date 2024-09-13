using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DataPoints.Queries;

public static class GetLatestDataPoints
{
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
                        string sensorTag
                    ) =>
                    {
                        var query = new Query(deviceId, sensorTag, user);
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

    public record Query(Guid DeviceId, string SensorTag, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext appContext, TimeScaleDbContext timescaleContext, RedisService redis) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var user = message.User;

            var device = appContext.Devices.Where(d => d.Id == message.DeviceId).FirstOrDefault();
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (device.OwnerId != user.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var redisKey = $"device:{message.DeviceId}:{message.SensorTag}:latest";
            var cachedLatestDataPoint = await redis.Db.StringGetAsync(redisKey);
            if (cachedLatestDataPoint.HasValue)
            {
                if (double.TryParse(cachedLatestDataPoint.ToString(), out double value))
                {
                    return Result.Ok(new Response(value));
                }
            }

            var latestDataPoint = await timescaleContext
                .DataPoints.Where(dp => dp.DeviceId == message.DeviceId && dp.SensorTag == message.SensorTag)
                .OrderByDescending(dp => dp.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
            if (latestDataPoint == null)
            {
                return Result.Fail(new NotFoundError());
            }

            await redis.Db.StringSetAsync(redisKey, latestDataPoint.Value.ToString(), TimeSpan.FromHours(1));
            var response = new Response(latestDataPoint.Value);
            return Result.Ok(response);
        }
    }

    public record Response(double? Value);
}
