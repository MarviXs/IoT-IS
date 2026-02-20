using System.Text.Json;
using Carter;
using EFCore.BulkExtensions;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Redis;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.DataPoints.Commands;

public static class CreateDataPoints
{
    public record Request(
        string Tag,
        double Value,
        long? TimeStamp,
        double? Latitude = null,
        double? Longitude = null,
        int? GridX = null,
        int? GridY = null
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/{deviceAccessToken}/data",
                    async Task<Results<NoContent, NotFound, ValidationProblem>> (
                        IMediator mediator,
                        List<Request> request,
                        string deviceAccessToken
                    ) =>
                    {
                        var command = new Command(request, deviceAccessToken);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(CreateDataPoints))
                .WithTags(nameof(DataPoint))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create data points";
                    o.Description = "This endpoint is called by the device to create data points.";
                    return o;
                });
        }
    }

    public record Command(List<Request> Request, string DeviceAccessToken) : IRequest<Result<Guid>>;

    public sealed class Handler(
        AppDbContext appContext,
        TimeScaleDbContext timescaleContext,
        RedisService redis,
        IHubContext<IsHub, INotificationsClient> hubContext
    ) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var device = await appContext
                .Devices.AsNoTracking()
                .FirstOrDefaultAsync(device => device.AccessToken == message.DeviceAccessToken, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            await UpdateSampleRateIfNeeded(appContext, device.Id, message.Request, cancellationToken);

            var dataPoints = message.Request.Select(dataPoint => new DataPoint
            {
                DeviceId = device.Id,
                SensorTag = dataPoint.Tag,
                TimeStamp = UnixTimestampUtils.NormalizeToDateTimeOffsetOrNow(dataPoint.TimeStamp),
                Value = dataPoint.Value,
                Latitude = dataPoint.Latitude,
                Longitude = dataPoint.Longitude,
                GridX = dataPoint.GridX,
                GridY = dataPoint.GridY
            });

            var deviceId = device.Id.ToString();
            foreach (var datapoint in dataPoints)
            {
                var streamEntries = new List<NameValueEntry>
                {
                    new("device_id", deviceId),
                    new("sensor_tag", datapoint.SensorTag),
                    new("value", datapoint.Value),
                    new("timestamp", datapoint.TimeStamp.ToUnixTimeMilliseconds()),
                };

                if (datapoint.Latitude.HasValue)
                {
                    streamEntries.Add(new("latitude", datapoint.Latitude.Value));
                }
                if (datapoint.Longitude.HasValue)
                {
                    streamEntries.Add(new("longitude", datapoint.Longitude.Value));
                }
                if (datapoint.GridX.HasValue)
                {
                    streamEntries.Add(new("grid_x", datapoint.GridX.Value));
                }
                if (datapoint.GridY.HasValue)
                {
                    streamEntries.Add(new("grid_y", datapoint.GridY.Value));
                }

                await redis.Db.StreamAddAsync("datapoints", streamEntries.ToArray(), maxLength: 500000);
                await redis.Db.StringSetAsync($"device:{deviceId}:connected", "1", TimeSpan.FromMinutes(30), flags: CommandFlags.FireAndForget);
                await redis.Db.StringSetAsync(
                    $"device:{deviceId}:lastSeen",
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    flags: CommandFlags.FireAndForget
                );
                var latestResponse = new GetLatestDataPoints.Response(
                    datapoint.TimeStamp,
                    datapoint.Value,
                    datapoint.Latitude,
                    datapoint.Longitude,
                    datapoint.GridX,
                    datapoint.GridY
                );

                await redis.Db.StringSetAsync(
                    $"device:{deviceId}:{datapoint.SensorTag}:last",
                    JsonSerializer.Serialize(latestResponse),
                    TimeSpan.FromHours(1),
                    flags: CommandFlags.FireAndForget
                );
                await hubContext
                    .Clients.Group(deviceId)
                    .ReceiveSensorLastDataPoint(
                        new SensorLastDataPointDto(
                            deviceId,
                            datapoint.SensorTag.ToString(),
                            datapoint.Value,
                            datapoint.Latitude,
                            datapoint.Longitude,
                            datapoint.GridX,
                            datapoint.GridY,
                            datapoint.TimeStamp
                        )
                    );
            }

            return Result.Ok();
        }

        private static async Task UpdateSampleRateIfNeeded(
            AppDbContext appContext,
            Guid deviceId,
            IReadOnlyCollection<Request> requests,
            CancellationToken cancellationToken
        )
        {
            var sampleRateRequest = requests.FirstOrDefault(request => string.Equals(request.Tag, "samplerate", StringComparison.OrdinalIgnoreCase));
            if (sampleRateRequest == null || double.IsNaN(sampleRateRequest.Value) || double.IsInfinity(sampleRateRequest.Value))
            {
                return;
            }
            if (sampleRateRequest.Value <= 0)
            {
                return;
            }

            var device = await appContext.Devices.FindAsync([deviceId], cancellationToken);
            if (device == null)
            {
                return;
            }

            device.SampleRateSeconds = (float)sampleRateRequest.Value;
            await appContext.SaveChangesAsync(cancellationToken);
        }
    }
}
