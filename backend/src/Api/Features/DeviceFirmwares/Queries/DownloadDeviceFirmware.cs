using System.IO;
using System.Net.Mime;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Services.DeviceFirmwares;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceFirmwares.Queries;

public static class DownloadDeviceFirmware
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates/{templateId:guid}/firmwares/{firmwareId:guid}/download",
                    async Task<Results<FileStreamHttpResult, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        HttpContext httpContext,
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

                        var (stream, originalFileName) = result.Value;

                        var contentDisposition = new ContentDisposition { FileName = originalFileName, Inline = false };

                        httpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
                        httpContext.Response.Headers.AccessControlExposeHeaders = "Content-Disposition";
                        httpContext.Response.ContentType = "application/octet-stream";

                        return TypedResults.File(stream);
                    }
                )
                .WithName(nameof(DownloadDeviceFirmware))
                .WithTags(nameof(DeviceFirmware))
                .WithOpenApi(o =>
                {
                    o.Summary = "Download a firmware file";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid TemplateId, Guid FirmwareId) : IRequest<Result<(Stream Stream, string FileName)>>;

    public sealed class Handler(AppDbContext context, IDeviceFirmwareFileService firmwareFileService)
        : IRequestHandler<Query, Result<(Stream Stream, string FileName)>>
    {
        public async Task<Result<(Stream Stream, string FileName)>> Handle(Query message, CancellationToken cancellationToken)
        {
            var firmware = await context
                .DeviceFirmwares.AsNoTracking()
                .Include(f => f.DeviceTemplate)
                .FirstOrDefaultAsync(f => f.Id == message.FirmwareId && f.DeviceTemplateId == message.TemplateId, cancellationToken);

            if (firmware == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (firmware.DeviceTemplate == null || firmware.DeviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var stream = await firmwareFileService.OpenReadAsync(firmware.StoredFileName, cancellationToken);
            return Result.Ok((stream, firmware.OriginalFileName));
        }
    }
}
