using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceControls.Queries;

public static class GetDeviceControls
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/controls",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid deviceId) =>
                    {
                        var query = new Query(user, deviceId);
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
                .WithName(nameof(GetDeviceControls))
                .WithTags(nameof(DeviceControl))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all controls on a device";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid DeviceId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context
                .Devices.Where(device => device.Id == request.DeviceId)
                .Include(device => device.SharedWithUsers)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            if (device != null)
            {
                if (!device.CanView(request.User))
                {
                    return Result.Fail(new ForbiddenError());
                }

                if (device.DeviceTemplateId is null)
                {
                    return Result.Ok(new List<Response>());
                }

                var template = await LoadTemplateWithControls(context, device.DeviceTemplateId.Value, cancellationToken);

                if (template == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                return Result.Ok(MapControls(template));
            }

            var templateFallback = await LoadTemplateWithControls(context, request.DeviceId, cancellationToken);

            if (templateFallback == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!templateFallback.IsOwner(request.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            return Result.Ok(MapControls(templateFallback));
        }

        private static async Task<DeviceTemplate?> LoadTemplateWithControls(
            AppDbContext context,
            Guid templateId,
            CancellationToken cancellationToken
        )
        {
            return await context
                .DeviceTemplates.Where(template => template.Id == templateId)
                .Include(template => template.Controls)
                .ThenInclude(control => control.Recipe)
                .Include(template => template.Controls)
                .ThenInclude(control => control.RecipeOn)
                .Include(template => template.Controls)
                .ThenInclude(control => control.RecipeOff)
                .Include(template => template.Controls)
                .ThenInclude(control => control.Sensor)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);
        }

        private static List<Response> MapControls(DeviceTemplate template)
        {
            return template
                .Controls.Select(control => new Response(
                    control.Id,
                    control.Name,
                    control.Color,
                    control.Type,
                    control.RecipeId,
                    control.Recipe?.Name ?? string.Empty,
                    control.Cycles,
                    control.IsInfinite,
                    control.RecipeOnId,
                    control.RecipeOn?.Name,
                    control.RecipeOffId,
                    control.RecipeOff?.Name,
                    control.SensorId,
                    control.Sensor?.Name,
                    control.Order
                ))
                .OrderBy(control => control.Order)
                .ToList();
        }
    }

    public record Response(
        Guid Id,
        string Name,
        string Color,
        DeviceControlType Type,
        Guid? RecipeId,
        string RecipeName,
        int Cycles,
        bool IsInfinite,
        Guid? RecipeOnId,
        string? RecipeOnName,
        Guid? RecipeOffId,
        string? RecipeOffName,
        Guid? SensorId,
        string? SensorName,
        int Order
    );
}
