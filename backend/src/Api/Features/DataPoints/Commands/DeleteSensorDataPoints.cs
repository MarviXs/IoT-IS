using System.Security.Claims;
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

namespace Fei.Is.Api.Features.DataPoints.Commands;

public static class DeleteSensorDataPoints
{
    public class CommandParameters
    {
        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "devices/{deviceId:guid}/sensors/{sensorTag}/data",
                    async Task<Results<Ok<Response>, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid deviceId,
                        string sensorTag,
                        [AsParameters] CommandParameters parameters
                    ) =>
                    {
                        var command = new Command(deviceId, sensorTag, user, parameters);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(DeleteSensorDataPoints))
                .WithTags(nameof(DataPoint))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete stored data points for a sensor";
                    o.Description =
                        "Delete stored data points for a sensor within an optional time range and return the number of removed records.";
                    return o;
                });
        }
    }

    public record Command(Guid DeviceId, string SensorTag, ClaimsPrincipal User, CommandParameters Parameters)
        : IRequest<Result<Response>>;

    public record Response(int DeletedCount);

    public sealed class Handler(
        AppDbContext appContext,
        TimeScaleDbContext timescaleContext,
        RedisService redis
    ) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var device = await appContext
                .Devices.Where(d => d.Id == message.DeviceId)
                .Include(d => d.SharedWithUsers)
                .FirstOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!device.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var from = message.Parameters.From;
            var to = message.Parameters.To;

            if (from.HasValue && to.HasValue && from > to)
            {
                return Result.Fail(new ValidationError(nameof(CommandParameters.From), "From must be earlier than To."));
            }

            var query = timescaleContext
                .DataPoints.Where(d => d.DeviceId == message.DeviceId && d.SensorTag == message.SensorTag);

            if (from.HasValue)
            {
                query = query.Where(d => d.TimeStamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(d => d.TimeStamp <= to.Value);
            }

            var deleted = await query.ExecuteDeleteAsync(cancellationToken);

            if (deleted > 0)
            {
                await redis.Db.KeyDeleteAsync($"device:{message.DeviceId}:{message.SensorTag}:last");
            }

            return Result.Ok(new Response(deleted));
        }
    }
}
