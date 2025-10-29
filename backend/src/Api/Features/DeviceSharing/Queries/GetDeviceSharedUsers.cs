using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceSharing.Queries;

public static class GetDeviceSharedUsers
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{id:guid}/shared-users",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(id, user);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetDeviceSharedUsers))
                .WithTags(nameof(DeviceShare))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a list of users with whom the device is shared";
                    return o;
                });
        }
    }

    public record Query(Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var device = await context.Devices.AsNoTracking().FirstOrDefaultAsync(device => device.Id == message.DeviceId, cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!message.User.IsAdmin() && device.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var sharedUsers = await context
                .DeviceShares.AsNoTracking()
                .Where(share => share.DeviceId == message.DeviceId)
                .Include(share => share.SharedToUser)
                .Select(share => new Response(share.SharedToUser!.Email ?? "Not found", share.Permission))
                .ToListAsync(cancellationToken);

            return Result.Ok(sharedUsers);
        }
    }

    public record Response(string Email, DeviceSharePermission Permission);
}
