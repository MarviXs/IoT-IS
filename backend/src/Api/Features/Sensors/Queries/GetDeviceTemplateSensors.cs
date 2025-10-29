using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Sensors.Queries;

public static class GetDeviceTemplateSensors
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{templateId:guid}/sensors",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid templateId) =>
                    {
                        var query = new Query(user, templateId);
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
                .WithName(nameof(GetDeviceTemplateSensors))
                .WithTags(nameof(Sensor))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all sensors on a device template";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid TemplateId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = request.User.GetUserId();

            var templateWithSensors = await context
                .DeviceTemplates.Where(template => template.Id == request.TemplateId)
                .Include(template => template.Sensors)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            if (templateWithSensors == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!templateWithSensors.IsOwner(request.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var sensors = templateWithSensors
                .Sensors.Select(sensor => new Response(
                    sensor.Id,
                    sensor.Tag,
                    sensor.Name,
                    sensor.Unit,
                    sensor.AccuracyDecimals,
                    sensor.Order,
                    sensor.Group
                ))
                .OrderBy(sensor => sensor.Order)
                .ToList();

            return Result.Ok(sensors);
        }
    }

    public record Response(Guid Id, string Tag, string Name, string? Unit, int? AccuracyDecimals, int Order, string? Group);
}
