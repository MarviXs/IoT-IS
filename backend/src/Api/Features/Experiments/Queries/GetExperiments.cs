using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Experiments.Queries;

public static class GetExperiments
{
    public class QueryParameters : SearchParameters
    {
        public Guid? RecipeId { get; init; }
        public Guid? RanJobId { get; init; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "experiments",
                    async Task<Results<Ok<PagedList<Response>>, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetExperiments))
                .WithTags(nameof(Experiment))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get experiments (paginated)";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, IValidator<QueryParameters> validator) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var qp = message.Parameters;

            var validationResult = validator.Validate(qp);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var query = context.Experiments.AsNoTracking();

            if (!message.User.IsAdmin())
            {
                var userId = message.User.GetUserId();
                query = query.Where(e => e.OwnerId == userId);
            }

            if (qp.RecipeId is Guid recipeId)
            {
                query = query.Where(e => e.RecipeToRunId == recipeId);
            }

            if (qp.RanJobId is Guid jobId)
            {
                query = query.Where(e => e.RanJobId == jobId);
            }

            if (!string.IsNullOrWhiteSpace(qp.SearchTerm))
            {
                var search = StringUtils.Normalized(qp.SearchTerm);
                query = query.Where(e => (e.Note ?? string.Empty).ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var experiments = await query
                .Sort(qp.SortBy ?? nameof(Experiment.UpdatedAt), qp.Descending)
                .Paginate(qp)
                .Select(e => new Response(e.Id, e.Note, e.DeviceId, e.RecipeToRunId, e.RanJobId, e.StartedAt, e.FinishedAt, e.CreatedAt, e.UpdatedAt))
                .ToListAsync(cancellationToken);

            return Result.Ok(experiments.ToPagedList(totalCount, qp.PageNumber, qp.PageSize));
        }
    }

    public record Response(
        Guid Id,
        string? Note,
        Guid? DeviceId,
        Guid? RecipeToRunId,
        Guid? RanJobId,
        DateTime? StartedAt,
        DateTime? FinishedAt,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields =
        [
            nameof(Experiment.CreatedAt),
            nameof(Experiment.UpdatedAt),
            nameof(Experiment.StartedAt),
            nameof(Experiment.FinishedAt)
        ];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
