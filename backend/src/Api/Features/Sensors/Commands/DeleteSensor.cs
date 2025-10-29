using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Sensors.Commands;

public static class DeleteSensor
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "sensors/{sensorId:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid sensorId) =>
                    {
                        var command = new Command(user, sensorId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteSensor))
                .WithTags(nameof(Sensor))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a sensor";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid SensorId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var sensor = await context
                .Sensors.Include(sensor => sensor.DeviceTemplate)
                .FirstOrDefaultAsync(sensor => sensor.Id == message.SensorId, cancellationToken);
            if (sensor == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (sensor.DeviceTemplate == null || !sensor.DeviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            context.Sensors.Remove(sensor);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
