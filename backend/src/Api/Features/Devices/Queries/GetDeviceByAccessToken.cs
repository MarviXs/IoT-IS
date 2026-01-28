using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.Devices.Queries;

public static class GetDeviceByAccessToken
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceAccessToken}/info",
                    async Task<Results<Ok<Response>, ValidationProblem, NotFound>> (IMediator mediator, string deviceAccessToken) =>
                    {
                        var query = new Query(deviceAccessToken);
                        var result = await mediator.Send(query);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(GetDeviceByAccessToken))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get device info by access token";
                    return o;
                });
        }
    }

    public record Query(string DeviceAccessToken) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, RedisService redis, IValidator<Query> validator) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var device = await context
                .Devices.AsNoTracking()
                .Include(d => d.DeviceTemplate)
                .ThenInclude(deviceTemplate => deviceTemplate!.Sensors)
                .FirstOrDefaultAsync(d => d.AccessToken == message.DeviceAccessToken, cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            RedisKey isOnlineKey = $"device:{device.Id}:connected";
            RedisKey lastSeenKey = $"device:{device.Id}:lastSeen";

            RedisValue isOnline = await redis.Db.StringGetAsync(isOnlineKey);
            RedisValue lastSeen = await redis.Db.StringGetAsync(lastSeenKey);

            var connectionState = device.GetConnectionState(
                isOnline.HasValue && isOnline == "1",
                lastSeen.HasValue && long.TryParse(lastSeen, out var timestampValue) ? DateTimeOffset.FromUnixTimeSeconds(timestampValue) : null,
                DateTimeOffset.UtcNow
            );

            var response = new Response(
                device.Id,
                device.Name,
                device.Mac,
                device.CurrentFirmwareVersion,
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
                        device.DeviceTemplate.EnableMap,
                        device.DeviceTemplate.EnableGrid,
                        device.DeviceTemplate.GridRowSpan,
                        device.DeviceTemplate.GridColumnSpan
                    )
                    : null,
                device.CreatedAt,
                device.UpdatedAt,
                isOnline.HasValue && isOnline == "1",
                lastSeen.HasValue && long.TryParse(lastSeen, out var timestamp) ? DateTimeOffset.FromUnixTimeSeconds(timestamp) : null,
                connectionState,
                device.Protocol,
                device.DataPointRetentionDays,
                device.SampleRateSeconds
            );

            return Result.Ok(response);
        }
    }

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.DeviceAccessToken).NotEmpty().WithMessage("Access token is required");
        }
    }

    public record SensorResponse(Guid Id, string Tag, string Name, string? Unit, int? AccuracyDecimals, int Order, string? Group);

    public record TemplateResponse(
        Guid Id,
        string Name,
        SensorResponse[] Sensors,
        DeviceType DeviceType,
        bool EnableMap,
        bool EnableGrid,
        int? GridRowSpan = null,
        int? GridColumnSpan = null
    );

    public record Response(
        Guid Id,
        string Name,
        string? Mac,
        string? CurrentFirmwareVersion,
        TemplateResponse? DeviceTemplate,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        bool Connected,
        DateTimeOffset? LastSeen,
        DeviceConnectionState ConnectionState,
        DeviceConnectionProtocol Protocol,
        int? DataPointRetentionDays,
        float? SampleRateSeconds
    );
}
