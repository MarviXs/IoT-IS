using System.Collections.Generic;
using System.Linq;
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

namespace Fei.Is.Api.Features.DeviceControls.Queries;

public static class GetDeviceTemplateControls
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{templateId:guid}/controls",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid templateId
                    ) =>
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
                .WithName(nameof(GetDeviceTemplateControls))
                .WithTags(nameof(DeviceControl))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all controls on a device template";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid TemplateId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var template = await context
                .DeviceTemplates.Where(template => template.Id == request.TemplateId)
                .Include(template => template.Controls)
                .ThenInclude(control => control.Recipe)
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (template.OwnerId != request.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var controls = template
                .Controls.Select(control => new Response(
                    control.Id,
                    control.Name,
                    control.Color,
                    control.RecipeId,
                    control.Recipe?.Name ?? string.Empty,
                    control.Cycles,
                    control.IsInfinite,
                    control.Order
                ))
                .OrderBy(control => control.Order)
                .ToList();

            return Result.Ok(controls);
        }
    }

    public record Response(
        Guid Id,
        string Name,
        string Color,
        Guid RecipeId,
        string RecipeName,
        int Cycles,
        bool IsInfinite,
        int Order
    );
}
