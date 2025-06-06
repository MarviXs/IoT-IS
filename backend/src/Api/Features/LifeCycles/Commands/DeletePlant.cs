using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.LifeCycles.Commands;

public static class DeletePlant
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "lifecycles/plant/{id:Guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(user, id);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeletePlant))
                .WithTags(nameof(Plant))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a plant";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid PlantId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var query = context.Plants.Where(plant => plant.Id == message.PlantId);

            if (!await query.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            context.Plants.RemoveRange(query);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
