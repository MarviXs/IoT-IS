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

namespace Fei.Is.Api.Features.Experiments.Queries;

public static class GetExperimentById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "experiments/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, Guid id, ClaimsPrincipal user) =>
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
                .WithName(nameof(GetExperimentById))
                .WithTags(nameof(Experiment))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get an experiment by id";
                    return o;
                });
        }
    }

    public record Query(Guid ExperimentId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var experiment = await context
                .Experiments.AsNoTracking()
                .Include(e => e.Device)
                .Include(e => e.RecipeToRun)
                .Include(e => e.RanJob)
                .FirstOrDefaultAsync(e => e.Id == request.ExperimentId, cancellationToken);

            if (experiment == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!request.User.IsAdmin() && experiment.OwnerId != request.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                experiment.Id,
                experiment.Note,
                experiment.DeviceId,
                experiment.Device?.Name,
                experiment.RecipeToRunId,
                experiment.RecipeToRun?.Name,
                experiment.RanJobId,
                experiment.RanJob?.Name,
                experiment.StartedAt,
                experiment.FinishedAt,
                experiment.CreatedAt,
                experiment.UpdatedAt
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        string? Note,
        Guid? DeviceId,
        string? DeviceName,
        Guid? RecipeToRunId,
        string? RecipeToRunName,
        Guid? RanJobId,
        string? RanJobName,
        DateTime? StartedAt,
        DateTime? FinishedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
