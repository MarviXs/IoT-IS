using System.Text.Json;
using Carter;
using EFCore.BulkExtensions;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Features.Devices.Services;
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
        DeviceAccessTokenResolver deviceAccessTokenResolver,
        RedisService redis,
        IHubContext<IsHub, INotificationsClient> hubContext
    ) : IRequestHandler<Command, Result<Guid>>
    {
        private const string DataPointsStreamName = "datapoints";
        private const string SampleRateTag = "samplerate";

        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var deviceId = await deviceAccessTokenResolver.ResolveDeviceIdAsync(message.DeviceAccessToken, cancellationToken);
            if (!deviceId.HasValue)
            {
                return Result.Fail(new NotFoundError());
            }

            await UpdateSampleRateIfNeeded(appContext, deviceId.Value, message.Request, cancellationToken);

            var requestCount = message.Request.Count;
            if (requestCount == 0)
            {
                return Result.Ok(deviceId.Value);
            }

            var deviceIdString = deviceId.Value.ToString();
            var notificationsGroup = hubContext.Clients.Group(deviceIdString);
            var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var redisBatch = redis.Db.CreateBatch();
            var pendingTasks = new List<Task>((requestCount * 2) + 3);
            var notifications = new List<SensorLastDataPointDto>(requestCount);
            pendingTasks.Add(redisBatch.StringSetAsync($"device:{deviceIdString}:connected", "1", TimeSpan.FromMinutes(30)));
            pendingTasks.Add(redisBatch.StringSetAsync($"device:{deviceIdString}:lastSeen", nowUnixSeconds));

            foreach (var request in message.Request)
            {
                var timestamp = UnixTimestampUtils.NormalizeToDateTimeOffsetOrNow(request.TimeStamp);
                var streamEntriesCount = 4;

                if (request.Latitude.HasValue)
                {
                    streamEntriesCount++;
                }
                if (request.Longitude.HasValue)
                {
                    streamEntriesCount++;
                }
                if (request.GridX.HasValue)
                {
                    streamEntriesCount++;
                }
                if (request.GridY.HasValue)
                {
                    streamEntriesCount++;
                }

                var streamEntries = new NameValueEntry[streamEntriesCount];
                var entryIndex = 0;
                streamEntries[entryIndex++] = new("device_id", deviceIdString);
                streamEntries[entryIndex++] = new("sensor_tag", request.Tag);
                streamEntries[entryIndex++] = new("value", request.Value);
                streamEntries[entryIndex++] = new("timestamp", timestamp.ToUnixTimeMilliseconds());

                if (request.Latitude.HasValue)
                {
                    streamEntries[entryIndex++] = new("latitude", request.Latitude.Value);
                }
                if (request.Longitude.HasValue)
                {
                    streamEntries[entryIndex++] = new("longitude", request.Longitude.Value);
                }
                if (request.GridX.HasValue)
                {
                    streamEntries[entryIndex++] = new("grid_x", request.GridX.Value);
                }
                if (request.GridY.HasValue)
                {
                    streamEntries[entryIndex++] = new("grid_y", request.GridY.Value);
                }

                pendingTasks.Add(redisBatch.StreamAddAsync(DataPointsStreamName, streamEntries, maxLength: 500000));

                var latestResponse = new GetLatestDataPoints.Response(
                    timestamp,
                    request.Value,
                    request.Latitude,
                    request.Longitude,
                    request.GridX,
                    request.GridY
                );

                pendingTasks.Add(redisBatch.StringSetAsync(
                    $"device:{deviceIdString}:{request.Tag}:last",
                    JsonSerializer.Serialize(latestResponse),
                    TimeSpan.FromHours(1)
                ));
                notifications.Add(
                    new SensorLastDataPointDto(
                        deviceIdString,
                        request.Tag,
                        request.Value,
                        request.Latitude,
                        request.Longitude,
                        request.GridX,
                        request.GridY,
                        timestamp
                    )
                );
            }

            pendingTasks.Add(notificationsGroup.ReceiveSensorLastDataPoints(notifications));
            redisBatch.Execute();
            await Task.WhenAll(pendingTasks);

            return Result.Ok(deviceId.Value);
        }

        private static Task UpdateSampleRateIfNeeded(
            AppDbContext appContext,
            Guid deviceId,
            IReadOnlyCollection<Request> requests,
            CancellationToken cancellationToken
        )
        {
            foreach (var request in requests)
            {
                if (!string.Equals(request.Tag, SampleRateTag, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (request.Value <= 0 || double.IsNaN(request.Value) || double.IsInfinity(request.Value))
                {
                    return Task.CompletedTask;
                }

                return UpdateSampleRateAsync(appContext, deviceId, request.Value, cancellationToken);
            }

            return Task.CompletedTask;
        }

        private static async Task UpdateSampleRateAsync(AppDbContext appContext, Guid deviceId, double value, CancellationToken cancellationToken)
        {
            var device = await appContext.Devices.FindAsync([deviceId], cancellationToken);
            if (device == null)
            {
                return;
            }

            device.SampleRateSeconds = (float)value;
            await appContext.SaveChangesAsync(cancellationToken);
        }
    }
}
