using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Queries;

public static class GetDeviceTemplateById
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{id:guid}",
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
                .WithName(nameof(GetDeviceTemplateById))
                .WithTags(nameof(DeviceTemplate))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a device template by id";
                    return o;
                });
        }
    }

    public record Query(Guid TemplateId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var template = await context
                .DeviceTemplates.AsNoTracking()
                .Include(t => t.SyncedFromEdgeNode)
                .FirstOrDefaultAsync(t => t.Id == message.TemplateId, cancellationToken);

            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }
            var canEdit = template.CanEdit(message.User);
            if (!canEdit && !template.IsGlobal)
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                template.Id,
                template.Name,
                template.DeviceType,
                template.EnableMap,
                template.EnableGrid,
                template.GridRowSpan,
                template.GridColumnSpan,
                template.IsGlobal,
                canEdit,
                template.SyncedFromEdge,
                template.SyncedFromEdgeNodeId,
                template.SyncedFromEdgeNode?.Name
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        string Name,
        DeviceType DeviceType,
        bool EnableMap,
        bool EnableGrid,
        int? GridRowSpan,
        int? GridColumnSpan,
        bool IsGlobal,
        bool CanEdit,
        bool SyncedFromEdge,
        Guid? SyncedFromEdgeNodeId,
        string? SyncedFromEdgeNodeName
    );
}
