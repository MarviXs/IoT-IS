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

public static class GetDeviceFirmwareById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{templateId:guid}/firmwares/{firmwareId:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid templateId,
                        Guid firmwareId
                    ) =>
                    {
                        var query = new Query(user, templateId, firmwareId);
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
                .WithName(nameof(GetDeviceFirmwareById))
                .WithTags(nameof(DeviceFirmware))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a firmware version by id";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid TemplateId, Guid FirmwareId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var firmware = await context
                .DeviceFirmwares.AsNoTracking()
                .Include(f => f.DeviceTemplate)
                .FirstOrDefaultAsync(
                    f => f.Id == message.FirmwareId && f.DeviceTemplateId == message.TemplateId,
                    cancellationToken
                );

            if (firmware == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (firmware.DeviceTemplate == null || firmware.DeviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                firmware.Id,
                firmware.VersionNumber,
                firmware.VersionDate,
                firmware.IsActive,
                firmware.OriginalFileName,
                BuildDownloadUrl(firmware.DeviceTemplateId, firmware.Id),
                firmware.CreatedAt,
                firmware.UpdatedAt
            );

            return Result.Ok(response);
        }
    }

    private static string BuildDownloadUrl(Guid templateId, Guid firmwareId) =>
        $"/api/device-templates/{templateId}/firmwares/{firmwareId}/download";

    public record Response(
        Guid Id,
        string VersionNumber,
        DateTime VersionDate,
        bool IsActive,
        string OriginalFileName,
        string DownloadUrl,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
