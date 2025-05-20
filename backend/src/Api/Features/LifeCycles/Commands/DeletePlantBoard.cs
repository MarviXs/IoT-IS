using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.LifeCycles.Commands;

public static class DeletePlantBoard
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "lifeboard/{id:guid}",
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
                .WithName(nameof(DeletePlantBoard))
                .WithTags(nameof(PlantBoard))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a plantboard";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid PlantBoardId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var query = context.PlantBoards.Where(plantBoard => plantBoard.PlantBoardId == message.PlantBoardId);

            if (!await query.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            context.PlantBoards.RemoveRange(query);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
