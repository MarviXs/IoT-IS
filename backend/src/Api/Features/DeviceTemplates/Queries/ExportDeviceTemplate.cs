using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.DeviceTemplates.Commands;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Queries;

public static class ExportDeviceTemplate
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{id:guid}/export",
                    async Task<Results<Ok<ImportDeviceTemplate.Request>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid id
                    ) =>
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
                .WithName(nameof(ExportDeviceTemplate))
                .WithTags(nameof(DeviceTemplate))
                .WithOpenApi(o =>
                {
                    o.Summary = "Export a device template";
                    o.Description = "Returns a payload compatible with the import device template endpoint.";
                    return o;
                });
        }
    }

    public record Query(Guid TemplateId, ClaimsPrincipal User) : IRequest<Result<ImportDeviceTemplate.Request>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<ImportDeviceTemplate.Request>>
    {
        public async Task<Result<ImportDeviceTemplate.Request>> Handle(Query message, CancellationToken cancellationToken)
        {
            var template = await context
                .DeviceTemplates.Include(t => t.Commands)
                .Include(t => t.Sensors)
                .Include(t => t.Controls)
                .ThenInclude(control => control.Recipe)
                .Include(t => t.Controls)
                .ThenInclude(control => control.RecipeOn)
                .Include(t => t.Controls)
                .ThenInclude(control => control.RecipeOff)
                .Include(t => t.Controls)
                .ThenInclude(control => control.Sensor)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == message.TemplateId, cancellationToken);

            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!template.CanEdit(message.User) && !template.IsGlobal)
            {
                return Result.Fail(new ForbiddenError());
            }

            var commands = template
                .Commands.Select(command => new ImportDeviceTemplate.DeviceCommandRequest(command.DisplayName, command.Name, command.Params))
                .ToList();

            var sensors = template
                .Sensors.OrderBy(sensor => sensor.Order)
                .Select(sensor => new ImportDeviceTemplate.SensorRequest(sensor.Tag, sensor.Name, sensor.Unit, sensor.AccuracyDecimals, sensor.Group))
                .ToList();

            var controls = template
                .Controls.OrderBy(control => control.Order)
                .Select(control => new ImportDeviceTemplate.DeviceControlRequest(
                    control.Name,
                    control.Color,
                    control.Type,
                    control.Recipe?.Name,
                    control.Cycles,
                    control.IsInfinite,
                    control.RecipeOn?.Name,
                    control.RecipeOff?.Name,
                    control.Sensor?.Tag,
                    control.Order
                ))
                .ToList();

            var templateData = new ImportDeviceTemplate.TemplateRequest(
                template.Name,
                commands,
                sensors,
                controls,
                template.DeviceType,
                template.EnableMap,
                template.EnableGrid,
                template.EnableGrid ? template.GridRowSpan : null,
                template.EnableGrid ? template.GridColumnSpan : null,
                template.IsGlobal
            );

            var response = new ImportDeviceTemplate.Request(templateData, "1.1");

            return Result.Ok(response);
        }
    }
}
