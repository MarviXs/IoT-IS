using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.Recipes.Commands;

public static class DeleteRecipe
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "recipes/{id:guid}",
                    async Task<Results<NoContent, NotFound>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteRecipe))
                .WithTags(nameof(Recipe))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a recipe";
                    return o;
                });
        }
    }

    public record Command(Guid Id, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var recipe = await context.Recipes.FindAsync([message.Id], cancellationToken);
            if (recipe == null)
            {
                return Result.Fail(new NotFoundError());
            }

            context.Recipes.Remove(recipe);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
