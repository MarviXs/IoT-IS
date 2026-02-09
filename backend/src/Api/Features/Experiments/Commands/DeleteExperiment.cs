using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Experiments.Commands;

public static class DeleteExperiment
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "experiments/{id:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var command = new Command(id, user);
                        var result = await mediator.Send(command);

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
                .WithName(nameof(DeleteExperiment))
                .WithTags(nameof(Experiment))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete an experiment";
                    return o;
                });
        }
    }

    public record Command(Guid ExperimentId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var experiment = await context.Experiments.FirstOrDefaultAsync(e => e.Id == message.ExperimentId, cancellationToken);
            if (experiment == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!message.User.IsAdmin() && experiment.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            context.Experiments.Remove(experiment);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
