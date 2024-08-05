using System.Security.Claims;
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

namespace Fei.Is.Api.Features.DeviceCollections.Commands;

public static class AddDeviceToDeviceCollection
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "device-collections/{collectionId:guid}/devices/{deviceId:guid}",
                    async Task<Results<Ok, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Guid collectionId, Guid deviceId) =>
                    {
                        var command = new Command(collectionId, deviceId, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok();
                    }
                )
                .WithName(nameof(AddDeviceToDeviceCollection))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Add a device to a device collection";
                    return o;
                });
        }
    }

    public record Command(Guid CollectionId, Guid DeviceId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var deviceCollection = await context.DeviceCollections.FindAsync([message.CollectionId], cancellationToken: cancellationToken);
            if (deviceCollection == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (deviceCollection.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var device = await context.Devices.FindAsync([message.DeviceId], cancellationToken: cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (device.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var existingCollectionItem = await context.CollectionItems.FirstOrDefaultAsync(
                ci => ci.CollectionParentId == deviceCollection.Id && ci.DeviceId == device.Id,
                cancellationToken
            );
            if (existingCollectionItem != null)
            {
                return Result.Ok();
            }

            var deviceCollectionItem = new CollectionItem { CollectionParentId = deviceCollection.Id, DeviceId = device.Id };

            context.CollectionItems.Add(deviceCollectionItem);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
