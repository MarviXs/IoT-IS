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
                    o.Description = "Delete stored data points for a sensor within an optional time range and return the number of removed records.";
                    return o;
                });
        }
    }

    public record Command(Guid DeviceId, string SensorTag, ClaimsPrincipal User, CommandParameters Parameters) : IRequest<Result<Response>>;

    public record Response(int DeletedCount);

    public sealed class Handler(AppDbContext appContext, TimeScaleDbContext timescaleContext, RedisService redis)
        : IRequestHandler<Command, Result<Response>>
    {
        private const int DeleteBatchSize = 50_000;

        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var device = await appContext
                .Devices.AsNoTracking()
                .Where(d => d.Id == message.DeviceId)
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

            var deleted = 0;
            while (true)
            {
                var deletedInBatch = await DeleteBatchAsync(message.DeviceId, message.SensorTag, from, to, cancellationToken);
                if (deletedInBatch == 0)
                {
                    break;
                }

                deleted = checked(deleted + deletedInBatch);

                if (deletedInBatch < DeleteBatchSize)
                {
                    break;
                }
            }

            if (deleted > 0)
            {
                await redis.Db.KeyDeleteAsync($"device:{message.DeviceId}:{message.SensorTag}:last");
            }

            return Result.Ok(new Response(deleted));
        }

        private async Task<int> DeleteBatchAsync(
            Guid deviceId,
            string sensorTag,
            DateTimeOffset? from,
            DateTimeOffset? to,
            CancellationToken cancellationToken
        )
        {
            FormattableString sql;

            if (from.HasValue && to.HasValue)
            {
                var fromValue = from.Value;
                var toValue = to.Value;
                sql =
                    $"""
                    WITH rows_to_delete AS (
                        SELECT dp.ctid
                        FROM "DataPoints" AS dp
                        WHERE dp."DeviceId" = {deviceId}
                          AND dp."SensorTag" = {sensorTag}
                          AND dp."TimeStamp" >= {fromValue}
                          AND dp."TimeStamp" <= {toValue}
                        ORDER BY dp."TimeStamp" DESC
                        LIMIT {DeleteBatchSize}
                    )
                    DELETE FROM "DataPoints" AS dp
                    USING rows_to_delete
                    WHERE dp.ctid = rows_to_delete.ctid;
                    """;
            }
            else if (from.HasValue)
            {
                var fromValue = from.Value;
                sql =
                    $"""
                    WITH rows_to_delete AS (
                        SELECT dp.ctid
                        FROM "DataPoints" AS dp
                        WHERE dp."DeviceId" = {deviceId}
                          AND dp."SensorTag" = {sensorTag}
                          AND dp."TimeStamp" >= {fromValue}
                        ORDER BY dp."TimeStamp" DESC
                        LIMIT {DeleteBatchSize}
                    )
                    DELETE FROM "DataPoints" AS dp
                    USING rows_to_delete
                    WHERE dp.ctid = rows_to_delete.ctid;
                    """;
            }
            else if (to.HasValue)
            {
                var toValue = to.Value;
                sql =
                    $"""
                    WITH rows_to_delete AS (
                        SELECT dp.ctid
                        FROM "DataPoints" AS dp
                        WHERE dp."DeviceId" = {deviceId}
                          AND dp."SensorTag" = {sensorTag}
                          AND dp."TimeStamp" <= {toValue}
                        ORDER BY dp."TimeStamp" DESC
                        LIMIT {DeleteBatchSize}
                    )
                    DELETE FROM "DataPoints" AS dp
                    USING rows_to_delete
                    WHERE dp.ctid = rows_to_delete.ctid;
                    """;
            }
            else
            {
                sql =
                    $"""
                    WITH rows_to_delete AS (
                        SELECT dp.ctid
                        FROM "DataPoints" AS dp
                        WHERE dp."DeviceId" = {deviceId}
                          AND dp."SensorTag" = {sensorTag}
                        ORDER BY dp."TimeStamp" DESC
                        LIMIT {DeleteBatchSize}
                    )
                    DELETE FROM "DataPoints" AS dp
                    USING rows_to_delete
                    WHERE dp.ctid = rows_to_delete.ctid;
                    """;
            }

            return await timescaleContext.Database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }
    }
}
