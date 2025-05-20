using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.GreenHouses.Commands;

public static class DeleteGreenHouse
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "greenhouses/{id:Guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(user, id);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    })
                .WithName(nameof(DeleteGreenHouse))
                .WithTags(nameof(GreenHouses))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a greenhouse";
                    return o;
                });
        }
    }

    public record Command(ClaimsPrincipal User, Guid GreenHouseId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var greenhouse = await context.Greenhouses
                .FirstOrDefaultAsync(g => g.Id == message.GreenHouseId, cancellationToken);

            if (greenhouse is null)
            {
                return Result.Fail(new NotFoundError());
            }

            context.Greenhouses.Remove(greenhouse);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
