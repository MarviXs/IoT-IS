using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Commands;

public static class DeleteDevice
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "devices/{id:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(user, id);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteDevice))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a device";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid DeviceId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var device = await context.Devices.Include(d => d.SharedWithUsers).FirstOrDefaultAsync(d => d.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.IsOwner(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            context.Devices.Remove(device);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
