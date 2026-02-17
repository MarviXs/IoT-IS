using System.Linq;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Commands;

public static class ChangeDeviceOwner
{
    public record Request(Guid OwnerId);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "admin/devices/{id:guid}/owner",
                    async Task<Results<NoContent, NotFound, ValidationProblem, ForbidHttpResult>> (IMediator mediator, Guid id, Request request) =>
                    {
                        var command = new Command(id, request);
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

                        return TypedResults.NoContent();
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(ChangeDeviceOwner))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Change device owner";
                    return o;
                });
        }
    }

    public record Command(Guid DeviceId, Request Request) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var device = await context.Devices.Include(d => d.SharedWithUsers).FirstOrDefaultAsync(d => d.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (device.SyncedFromEdge)
            {
                return Result.Fail(new ForbiddenError());
            }

            var newOwner = await context.Users.FirstOrDefaultAsync(u => u.Id == message.Request.OwnerId, cancellationToken);
            if (newOwner == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (device.SharedWithUsers?.Any() == true)
            {
                var sharesToRemove = device.SharedWithUsers.Where(share => share.SharedToUserId == message.Request.OwnerId).ToList();

                if (sharesToRemove.Count > 0)
                {
                    context.DeviceShares.RemoveRange(sharesToRemove);
                }
            }

            device.OwnerId = message.Request.OwnerId;
            device.Owner = newOwner;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.DeviceId).NotEmpty().WithMessage("Device ID is required");
            RuleFor(command => command.Request.OwnerId).NotEmpty().WithMessage("Owner ID is required");
        }
    }
}
