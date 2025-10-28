using System.IO;
using System.Linq;
using System.Net.Mime;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Services.DeviceFirmwares;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Queries;

public static class DownloadDeviceFirmware
{
    public record Request(string AccessToken);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/firmwares/{firmwareId:guid}/download",
                    async Task<Results<FileStreamHttpResult, ValidationProblem, NotFound>> (
                        IMediator mediator,
                        HttpContext httpContext,
                        Guid firmwareId,
                        [AsParameters] Request request
                    ) =>
                    {
                        var query = new Query(request, firmwareId);
                        var result = await mediator.Send(query);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        var (stream, fileName) = result.Value;

                        var contentDisposition = new ContentDisposition { FileName = fileName, Inline = false };

                        httpContext.Response.Headers.ContentDisposition = contentDisposition.ToString();
                        httpContext.Response.Headers.AccessControlExposeHeaders = "Content-Disposition";
                        httpContext.Response.ContentType = "application/octet-stream";

                        return TypedResults.File(stream);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(DownloadDeviceFirmware))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Download a firmware file using device access token";
                    return o;
                });
        }
    }

    public record Query(Request Request, Guid FirmwareId) : IRequest<Result<(Stream Stream, string FileName)>>;

    public sealed class Handler(AppDbContext context, IDeviceFirmwareFileService firmwareFileService, IValidator<Query> validator)
        : IRequestHandler<Query, Result<(Stream Stream, string FileName)>>
    {
        public async Task<Result<(Stream Stream, string FileName)>> Handle(Query message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var deviceInfo = await context
                .Devices.AsNoTracking()
                .Where(d => d.AccessToken == message.Request.AccessToken)
                .Select(d => new { d.DeviceTemplateId })
                .FirstOrDefaultAsync(cancellationToken);

            if (deviceInfo == null || !deviceInfo.DeviceTemplateId.HasValue)
            {
                return Result.Fail(new NotFoundError());
            }

            var firmware = await context
                .DeviceFirmwares.AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == message.FirmwareId && f.DeviceTemplateId == deviceInfo.DeviceTemplateId.Value, cancellationToken);

            if (firmware == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var stream = await firmwareFileService.OpenReadAsync(firmware.StoredFileName, cancellationToken);
            return Result.Ok((stream, firmware.OriginalFileName));
        }
    }

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Request.AccessToken).NotEmpty().WithMessage("Access token is required");
        }
    }
}
