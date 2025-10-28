using System.Linq;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Services.DeviceFirmwares;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceFirmwares.Commands;

public static class UpdateDeviceFirmware
{
    public record Request(string VersionNumber, bool IsActive, IFormFile? FirmwareFile);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "device-templates/{templateId:guid}/firmwares/{firmwareId:guid}",
                    async Task<Results<Ok, ValidationProblem, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [FromForm] Request request,
                        Guid templateId,
                        Guid firmwareId
                    ) =>
                    {
                        var command = new Command(request, user, templateId, firmwareId);
                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok();
                    }
                )
                .DisableAntiforgery()
                .WithName(nameof(UpdateDeviceFirmware))
                .WithTags(nameof(DeviceFirmware))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a firmware version for a device template";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User, Guid TemplateId, Guid FirmwareId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator, IDeviceFirmwareFileService firmwareFileService)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var firmware = await context
                .DeviceFirmwares.Include(f => f.DeviceTemplate)
                .FirstOrDefaultAsync(f => f.Id == message.FirmwareId && f.DeviceTemplateId == message.TemplateId, cancellationToken);

            if (firmware == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (firmware.DeviceTemplate == null || firmware.DeviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            bool versionExists = await context.DeviceFirmwares.AnyAsync(
                f => f.DeviceTemplateId == message.TemplateId && f.VersionNumber == message.Request.VersionNumber && f.Id != message.FirmwareId,
                cancellationToken
            );

            if (versionExists)
            {
                return Result.Fail(new ValidationError("VersionNumber", "Version number already exists for this template"));
            }

            if (message.Request.IsActive)
            {
                var otherFirmwares = await context
                    .DeviceFirmwares.Where(f => f.DeviceTemplateId == message.TemplateId && f.Id != message.FirmwareId && f.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var otherFirmware in otherFirmwares)
                {
                    otherFirmware.IsActive = false;
                }
            }

            if (message.Request.FirmwareFile != null)
            {
                var storedFileName = await firmwareFileService.SaveAsync(message.Request.FirmwareFile, firmware.StoredFileName, cancellationToken);
                firmware.StoredFileName = storedFileName;
                firmware.OriginalFileName = message.Request.FirmwareFile.FileName;
            }

            firmware.VersionNumber = message.Request.VersionNumber;
            firmware.IsActive = message.Request.IsActive;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.VersionNumber).NotEmpty().WithMessage("Version number is required");
        }
    }
}
