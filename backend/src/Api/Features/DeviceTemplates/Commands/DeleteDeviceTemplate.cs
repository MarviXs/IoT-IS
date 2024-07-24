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

namespace Fei.Is.Api.Features.DeviceTemplates.Commands;

public static class DeleteDeviceTemplate
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "device-templates/{id:guid}",
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
                .WithName(nameof(DeleteDeviceTemplate))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a device template";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid TemplateId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var deviceTemplate = await context.DeviceTemplates.FindAsync([message.TemplateId], cancellationToken);
            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (deviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            context.DeviceTemplates.Remove(deviceTemplate);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
