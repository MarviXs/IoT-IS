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

public static class CreateDeviceFirmware
{
    public record Request(string VersionNumber, bool IsActive, IFormFile FirmwareFile);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "device-templates/{templateId:guid}/firmwares",
                    async Task<Results<Created<Guid>, ValidationProblem, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [FromForm] Request request,
                        Guid templateId
                    ) =>
                    {
                        var command = new Command(request, user, templateId);
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

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .DisableAntiforgery()
                .WithName(nameof(CreateDeviceFirmware))
                .WithTags(nameof(DeviceFirmware))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a firmware version for a device template";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User, Guid TemplateId) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator, IDeviceFirmwareFileService firmwareFileService)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var deviceTemplate = await context
                .DeviceTemplates.Include(template => template.Firmwares)
                .FirstOrDefaultAsync(template => template.Id == message.TemplateId, cancellationToken);

            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (deviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            bool versionExists = await context.DeviceFirmwares.AnyAsync(
                firmware => firmware.DeviceTemplateId == message.TemplateId && firmware.VersionNumber == message.Request.VersionNumber,
                cancellationToken
            );

            if (versionExists)
            {
                return Result.Fail(new ValidationError("VersionNumber", "Version number already exists for this template"));
            }

            var storedFileName = await firmwareFileService.SaveAsync(message.Request.FirmwareFile, null, cancellationToken);

            if (message.Request.IsActive)
            {
                foreach (var firmware in deviceTemplate.Firmwares.Where(f => f.IsActive))
                {
                    firmware.IsActive = false;
                }
            }

            var deviceFirmware = new DeviceFirmware
            {
                DeviceTemplateId = message.TemplateId,
                VersionNumber = message.Request.VersionNumber,
                IsActive = message.Request.IsActive,
                OriginalFileName = message.Request.FirmwareFile.FileName,
                StoredFileName = storedFileName
            };

            await context.DeviceFirmwares.AddAsync(deviceFirmware, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(deviceFirmware.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.VersionNumber).NotEmpty().WithMessage("Version number is required");

            RuleFor(command => command.Request.FirmwareFile).NotNull().WithMessage("Firmware file is required");
        }
    }
}
