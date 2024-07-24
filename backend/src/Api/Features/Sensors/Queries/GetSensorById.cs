using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Sensors.Queries;

public static class GetSensorById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "sensors/{sensorId:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid sensorId) =>
                    {
                        var query = new Query(user, sensorId);
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
                .WithName(nameof(GetSensorById))
                .WithTags(nameof(Sensor))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a sensor by ID";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid SensorId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = request.User.GetUserId();

            var sensor = await context
                .Sensors.AsNoTracking()
                .Include(sensor => sensor.DeviceTemplate)
                .FirstOrDefaultAsync(sensor => sensor.Id == request.SensorId, cancellationToken);
            if (sensor == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (sensor.DeviceTemplate == null || sensor.DeviceTemplate.OwnerId != userId)
            {
                return Result.Fail(new ForbiddenError());
            }

            return Result.Ok(new Response(sensor.Id, sensor.Tag, sensor.Name, sensor.Unit, sensor.AccuracyDecimals));
        }
    }

    public record Response(Guid Id, string Tag, string Name, string? Unit, int? AccuracyDecimals);
}
