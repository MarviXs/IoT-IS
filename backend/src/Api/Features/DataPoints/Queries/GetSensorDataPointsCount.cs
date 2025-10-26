using System.Security.Claims;
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

public static class GetSensorDataPointsCount
{
    public class QueryParameters
    {
        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/sensors/{sensorTag}/data/count",
                    async Task<Results<Ok<Response>, NotFound, ValidationProblem, ForbidHttpResult>> (
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

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetSensorDataPointsCount))
                .WithTags(nameof(DataPoint))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get the number of stored data points for a sensor";
                    o.Description =
                        "Return the total number of stored data points for a sensor within an optional time range.";
                    return o;
                });
        }
    }

    public record Query(Guid DeviceId, string SensorTag, ClaimsPrincipal User, QueryParameters Parameters)
        : IRequest<Result<Response>>;

    public record Response(long TotalCount);

    public sealed class Handler(AppDbContext appContext, TimeScaleDbContext timescaleContext)
        : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
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
                return Result.Fail(new ValidationError(nameof(QueryParameters.From), "From must be earlier than To."));
            }

            var query = timescaleContext
                .DataPoints.AsNoTracking()
                .Where(d => d.DeviceId == message.DeviceId && d.SensorTag == message.SensorTag);

            if (from.HasValue)
            {
                query = query.Where(d => d.TimeStamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(d => d.TimeStamp <= to.Value);
            }

            var total = await query.LongCountAsync(cancellationToken);

            return Result.Ok(new Response(total));
        }
    }
}
