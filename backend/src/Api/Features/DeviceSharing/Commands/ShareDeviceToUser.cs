using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceSharing.Commands;

public static class ShareDeviceToUser
{
    public record Request(string Email, DeviceSharePermission Permission);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/{deviceId:guid}/share",
                    async Task<Results<Created<Guid>, NotFound, ForbidHttpResult, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
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
                .WithName(nameof(ShareDeviceToUser))
                .WithTags(nameof(DeviceShare))
                .WithOpenApi(o =>
                {
                    o.Summary = "Share a device with a user by email";
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
            if (!device.IsOwner(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == message.Request.Email, cancellationToken);
            if (user == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var deviceShare = new DeviceShare
            {
                DeviceId = device.Id,
                Device = device,
                SharedToUser = user,
                Permission = message.Request.Permission
            };

            await context.DeviceShares.AddAsync(deviceShare, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(deviceShare.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().WithMessage("Email is required");
            RuleFor(x => x.Request.Permission).IsInEnum().WithMessage("Permission is invalid");
        }
    }
}
