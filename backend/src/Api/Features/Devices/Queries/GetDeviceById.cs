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

namespace Fei.Is.Api.Features.Devices.Queries;

public static class GetDeviceById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(id, user);
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
                .WithName(nameof(GetDeviceById))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a device by id";
                    return o;
                });
        }
    }

    public record Query(Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context
                .Devices.AsNoTracking()
                .Include(device => device.DeviceTemplate)
                .ThenInclude(deviceTemplate => deviceTemplate!.Sensors)
                .FirstOrDefaultAsync(device => device.Id == request.DeviceId, cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (device.OwnerId != request.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                device.Id,
                device.Name,
                device.Mac,
                device.AccessToken,
                device.DeviceTemplate != null ? new TemplateResponse(
                    device.DeviceTemplate.Id,
                    device.DeviceTemplate.Name,
                    device
                        .DeviceTemplate.Sensors.Select(
                            sensor => new SensorResponse(sensor.Id, sensor.Tag, sensor.Name, sensor.Unit, sensor.AccuracyDecimals)
                        )
                        .ToArray()
                ) : null,
                device.CreatedAt,
                device.UpdatedAt
            );

            return Result.Ok(response);
        }
    }

    public record SensorResponse(Guid Id, string Tag, string Name, string? Unit, int? AccuracyDecimals);

    public record TemplateResponse(Guid Id, string Name, SensorResponse[] Sensors);

    public record Response(
        Guid Id,
        string Name,
        string? Mac,
        string? AccessToken,
        TemplateResponse? DeviceTemplate,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
