using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Services.DeviceFirmwares;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceFirmwares.Commands;

public static class DeleteDeviceFirmware
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "device-templates/{templateId:guid}/firmwares/{firmwareId:guid}",
                    async Task<Results<Ok, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid templateId,
                        Guid firmwareId
                    ) =>
                    {
                        var command = new Command(user, templateId, firmwareId);
                        var result = await mediator.Send(command);

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
                .WithName(nameof(DeleteDeviceFirmware))
                .WithTags(nameof(DeviceFirmware))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a firmware version for a device template";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid TemplateId, Guid FirmwareId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IDeviceFirmwareFileService firmwareFileService) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
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

            context.DeviceFirmwares.Remove(firmware);
            await context.SaveChangesAsync(cancellationToken);

            firmwareFileService.Delete(firmware.StoredFileName);

            return Result.Ok();
        }
    }
}
