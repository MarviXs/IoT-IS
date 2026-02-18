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
using Npgsql;
using NpgsqlTypes;

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
        private const string DeleteBatchSql = """
            WITH candidates AS (
                SELECT dp.tableoid, dp."DeviceId", dp."SensorTag", dp."TimeStamp"
                FROM "DataPoints" AS dp
                WHERE dp."DeviceId" = @device_id
                  AND dp."SensorTag" = @sensor_tag
                  AND (@from_ts IS NULL OR dp."TimeStamp" >= @from_ts)
                  AND (@to_ts IS NULL OR dp."TimeStamp" <= @to_ts)
                ORDER BY dp."TimeStamp" DESC
                LIMIT @batch_size
            )
            DELETE FROM "DataPoints" AS dp
            USING candidates
            WHERE dp.tableoid = candidates.tableoid
              AND dp."DeviceId" = candidates."DeviceId"
              AND dp."SensorTag" = candidates."SensorTag"
              AND dp."TimeStamp" = candidates."TimeStamp";
            """;

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
            var parameters = new[]
            {
                new NpgsqlParameter("device_id", deviceId),
                new NpgsqlParameter("sensor_tag", sensorTag),
                new NpgsqlParameter("from_ts", NpgsqlDbType.TimestampTz) { Value = from ?? (object)DBNull.Value },
                new NpgsqlParameter("to_ts", NpgsqlDbType.TimestampTz) { Value = to ?? (object)DBNull.Value },
                new NpgsqlParameter("batch_size", DeleteBatchSize)
            };

            return await timescaleContext.Database.ExecuteSqlRawAsync(DeleteBatchSql, parameters, cancellationToken);
        }
    }
}
