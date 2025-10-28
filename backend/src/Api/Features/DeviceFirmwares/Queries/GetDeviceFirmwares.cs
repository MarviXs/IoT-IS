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

namespace Fei.Is.Api.Features.DeviceFirmwares.Queries;

public static class GetDeviceFirmwares
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{templateId:guid}/firmwares",
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
                .WithName(nameof(GetDeviceFirmwares))
                .WithTags(nameof(DeviceFirmware))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get firmware versions for a device template";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid TemplateId) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var deviceTemplate = await context
                .DeviceTemplates.AsNoTracking()
                .Include(template => template.Firmwares)
                .FirstOrDefaultAsync(template => template.Id == message.TemplateId, cancellationToken);

            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (deviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var firmwares = deviceTemplate
                .Firmwares
                .OrderByDescending(f => f.CreatedAt)
                .Select(
                    firmware =>
                        new Response(
                            firmware.Id,
                            firmware.VersionNumber,
                            firmware.IsActive,
                            firmware.OriginalFileName,
                            BuildDownloadUrl(deviceTemplate.Id, firmware.Id),
                            firmware.CreatedAt,
                            firmware.UpdatedAt
                        )
                )
                .ToList();

            return Result.Ok(firmwares);
        }
    }

    private static string BuildDownloadUrl(Guid templateId, Guid firmwareId) =>
        $"/api/device-templates/{templateId}/firmwares/{firmwareId}/download";

    public record Response(
        Guid Id,
        string VersionNumber,
        bool IsActive,
        string OriginalFileName,
        string DownloadUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
