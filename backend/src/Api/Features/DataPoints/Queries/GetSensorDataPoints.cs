using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DataPoints.Queries;

public static class GetSensorDataPoints
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DownsampleMethod
    {
        Asap,
        Lttb
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TimeBucketMethod
    {
        Average,
        Max,
        Min,
        StdDev,
        Sum
    }

    public class QueryParameters
    {
        [FromQuery]
        public bool? OnlyWithLocation { get; set; }

        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public int? Downsample { get; set; }
        public double? TimeBucket { get; set; }

        [FromQuery]
        public DownsampleMethod? DownsampleMethod { get; set; }

        [FromQuery]
        public TimeBucketMethod? TimeBucketMethod { get; set; }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/sensors/{sensorTag}/data",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        string sensorTag,
                        Guid deviceId,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(parameters, deviceId, sensorTag, user);
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
                .WithName(nameof(GetSensorDataPoints))
                .WithTags(nameof(DataPoint))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get stored data points for a sensor";
                    o.Description =
                        "Get stored data points for a sensor within a specified time range. Optionally downsample the data or group it into time buckets.";
                    return o;
                });
        }
    }

    public record Query(QueryParameters Parameters, Guid DeviceId, string SensorTag, ClaimsPrincipal User) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext appContext, TimeScaleDbContext timescaleContext) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var parameters = message.Parameters;

            var device = appContext.Devices.Where(d => d.Id == message.DeviceId).Include(d => d.SharedWithUsers).FirstOrDefault();
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.CanView(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var from = parameters.From ?? DateTimeOffset.MinValue;
            var to = parameters.To ?? DateTimeOffset.UtcNow;

            IQueryable<Response> query;

            if (parameters.Downsample.HasValue)
            {
                query = GetQueryBasedOnDownsample(message);
            }
            else if (parameters.TimeBucket.HasValue)
            {
                query = GetQueryBasedOnTimeBucket(message);
            }
            else
            {
                var baseQuery = timescaleContext
                    .DataPoints.Where(d => d.SensorTag == message.SensorTag && d.DeviceId == message.DeviceId)
                    .Where(d => d.TimeStamp >= from && d.TimeStamp <= to);

                if (parameters.OnlyWithLocation == true)
                {
                    baseQuery = baseQuery.Where(d => d.Latitude != null && d.Longitude != null);
                }

                query = baseQuery.OrderByDescending(d => d.TimeStamp).Select(d => new Response(d.TimeStamp, d.Value));
            }

            var response = await query.ToListAsync(cancellationToken);

            return Result.Ok(response);
        }

        private IQueryable<Response> GetQueryBasedOnDownsample(Query message)
        {
            var parameters = message.Parameters;
            var from = parameters.From ?? DateTimeOffset.MinValue;
            var to = parameters.To ?? DateTimeOffset.UtcNow;

            FormattableString queryString;

            switch (parameters.DownsampleMethod)
            {
                case DownsampleMethod.Asap:
                    queryString =
                        $@"
                        SELECT time as ""TimeStamp"", value as ""Value"" 
                        FROM unnest(
                            (
                                SELECT asap_smooth(""TimeStamp"", ""Value"", {parameters.Downsample}) 
                                FROM ""DataPoints"" 
                                WHERE ""SensorTag"" = {message.SensorTag}
                                AND ""DeviceId"" = {message.DeviceId}
                                AND ""TimeStamp"" >= {from} 
                                AND ""TimeStamp"" <= {to}
                            )
                        )";
                    break;
                default:
                    queryString =
                        $@"
                        SELECT time as ""TimeStamp"", value as ""Value"" 
                        FROM unnest(
                            (
                                SELECT lttb(""TimeStamp"", ""Value"", {parameters.Downsample}) 
                                FROM ""DataPoints"" 
                                WHERE ""SensorTag"" = {message.SensorTag}
                                AND ""DeviceId"" = {message.DeviceId}
                                AND ""TimeStamp"" >= {from} 
                                AND ""TimeStamp"" <= {to}
                            )
                        )";
                    break;
            }

            return timescaleContext.DataPoints.FromSql(queryString).Select(d => new Response(d.TimeStamp, d.Value));
        }

        private IQueryable<Response> GetQueryBasedOnTimeBucket(Query message)
        {
            var parameters = message.Parameters;
            var from = parameters.From ?? DateTimeOffset.MinValue;
            var to = parameters.To ?? DateTimeOffset.UtcNow;

            var timespan = TimeSpan.FromSeconds(parameters.TimeBucket!.Value);
            var baseQuery = timescaleContext
                .DataPoints.Where(d => d.SensorTag == message.SensorTag && d.DeviceId == message.DeviceId)
                .Where(d => d.TimeStamp >= from && d.TimeStamp <= to)
                .GroupBy(d => TimeScaleDbContext.TimeBucket(timespan, d.TimeStamp))
                .OrderByDescending(g => g.Key);

            IQueryable<Response> responseQuery = message.Parameters.TimeBucketMethod switch
            {
                TimeBucketMethod.Sum => baseQuery.Select(g => new Response(g.Key, g.Sum(d => d.Value))),
                TimeBucketMethod.Max => baseQuery.Select(g => new Response(g.Key, g.Max(d => d.Value))),
                TimeBucketMethod.Min => baseQuery.Select(g => new Response(g.Key, g.Min(d => d.Value))),
                TimeBucketMethod.StdDev
                    => baseQuery.Select(g => new Response(
                        g.Key,
                        NpgsqlAggregateDbFunctionsExtensions.StandardDeviationSample(EF.Functions, g.Select(d => (double)d.Value!))
                    )),
                _ => baseQuery.Select(g => new Response(g.Key, g.Average(d => d.Value))),
            };

            return responseQuery;
        }
    }

    public record Response(DateTimeOffset Ts, double? Value);
}
