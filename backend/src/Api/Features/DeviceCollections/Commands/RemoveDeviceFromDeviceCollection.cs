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

public static class RemoveDeviceFromDeviceCollection
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "device-collections/{collectionId:guid}/devices/{deviceId:guid}",
                    async Task<Results<NoContent, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid collectionId,
                        Guid deviceId
                    ) =>
                    {
                        var command = new Command(collectionId, deviceId, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(RemoveDeviceFromDeviceCollection))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Remove device from device collection";
                    return o;
                });
        }
    }

    public record Command(Guid CollectionId, Guid DeviceId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var collection = await context.DeviceCollections.FindAsync([message.CollectionId], cancellationToken);
            if (collection == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (collection.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var item = await context.CollectionItems.FirstOrDefaultAsync(
                ci => ci.CollectionParentId == message.CollectionId && ci.DeviceId == message.DeviceId,
                cancellationToken
            );
            if (item == null)
            {
                return Result.Ok();
            }

            context.CollectionItems.Remove(item);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
