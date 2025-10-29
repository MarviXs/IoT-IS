using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceSharing.Commands;

public static class UnshareDeviceToUser
{
    public record Request(string Email);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/{deviceId:guid}/unshare",
                    async Task<Results<Created<Guid>, ValidationProblem, NotFound>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [FromBody] Request request,
                        Guid deviceId
                    ) =>
                    {
                        var command = new Command(request, deviceId, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(UnshareDeviceToUser))
                .WithTags(nameof(DeviceShare))
                .WithOpenApi(o =>
                {
                    o.Summary = "Unshare a device with a user by email";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var device = await context.Devices.FirstOrDefaultAsync(x => x.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!message.User.IsAdmin() && device.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var sharedUser = await context.Users.FirstOrDefaultAsync(x => x.Email == message.Request.Email, cancellationToken);
            if (sharedUser == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var deviceShare = await context.DeviceShares.FirstOrDefaultAsync(
                x => x.DeviceId == message.DeviceId && x.SharedToUserId == sharedUser.Id,
                cancellationToken
            );

            if (deviceShare == null)
            {
                return Result.Fail(new NotFoundError());
            }

            context.DeviceShares.Remove(deviceShare);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(deviceShare.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().WithMessage("Email is required");
        }
    }
}
