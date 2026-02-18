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
using StackExchange.Redis;

namespace Fei.Is.Api.Features.DataPoints.Commands;

public static class DeleteSensorDataPoints
{
    public class CommandParameters
    {
        public string[]? SensorTags { get; set; }

        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "devices/{deviceId:guid}/data",
                    async Task<Results<Ok<Response>, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid deviceId,
                        [AsParameters] CommandParameters parameters
                    ) =>
                    {
                        var command = new Command(deviceId, user, parameters);
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
                    o.Summary = "Delete stored data points for sensors";
                    o.Description = "Delete stored data points for selected sensors within an optional time range and return the number of removed records.";
                    return o;
                });
        }
    }

    public record Command(Guid DeviceId, ClaimsPrincipal User, CommandParameters Parameters) : IRequest<Result<Response>>;

    public record Response(int DeletedCount);

    public sealed class Handler(AppDbContext appContext, TimeScaleDbContext timescaleContext, RedisService redis)
        : IRequestHandler<Command, Result<Response>>
    {
        private const int DeleteBatchSize = 25_000;
        private const string DeleteBatchSql = """
            WITH candidates AS (
                SELECT
                    dp.tableoid,
                    dp."DeviceId",
                    dp."SensorTag",
                    dp."TimeStamp",
                    dp."Value",
                    dp."Latitude",
                    dp."Longitude",
                    dp."GridX",
                    dp."GridY"
                FROM "DataPoints" AS dp
                WHERE dp."DeviceId" = @device_id
                  AND dp."SensorTag" = ANY(@sensor_tags)
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
              AND dp."TimeStamp" = candidates."TimeStamp"
              AND dp."Value" IS NOT DISTINCT FROM candidates."Value"
              AND dp."Latitude" IS NOT DISTINCT FROM candidates."Latitude"
              AND dp."Longitude" IS NOT DISTINCT FROM candidates."Longitude"
              AND dp."GridX" IS NOT DISTINCT FROM candidates."GridX"
              AND dp."GridY" IS NOT DISTINCT FROM candidates."GridY";
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
            var sensorTags = (message.Parameters.SensorTags ?? [])
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (sensorTags.Length == 0)
            {
                return Result.Fail(new ValidationError(nameof(CommandParameters.SensorTags), "At least one sensor tag must be provided."));
            }

            if (from.HasValue && to.HasValue && from > to)
            {
                return Result.Fail(new ValidationError(nameof(CommandParameters.From), "From must be earlier than To."));
            }

            var deleted = 0;
            while (true)
            {
                var deletedInBatch = await DeleteBatchAsync(message.DeviceId, sensorTags, from, to, cancellationToken);
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
                var redisKeys = sensorTags.Select(tag => (RedisKey)$"device:{message.DeviceId}:{tag}:last").ToArray();
                await redis.Db.KeyDeleteAsync(redisKeys);
            }

            return Result.Ok(new Response(deleted));
        }

        private async Task<int> DeleteBatchAsync(
            Guid deviceId,
            string[] sensorTags,
            DateTimeOffset? from,
            DateTimeOffset? to,
            CancellationToken cancellationToken
        )
        {
            var parameters = new[]
            {
                new NpgsqlParameter("device_id", deviceId),
                new NpgsqlParameter("sensor_tags", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = sensorTags },
                new NpgsqlParameter("from_ts", NpgsqlDbType.TimestampTz) { Value = from ?? (object)DBNull.Value },
                new NpgsqlParameter("to_ts", NpgsqlDbType.TimestampTz) { Value = to ?? (object)DBNull.Value },
                new NpgsqlParameter("batch_size", DeleteBatchSize)
            };

            return await timescaleContext.Database.ExecuteSqlRawAsync(DeleteBatchSql, parameters, cancellationToken);
        }
    }
}
