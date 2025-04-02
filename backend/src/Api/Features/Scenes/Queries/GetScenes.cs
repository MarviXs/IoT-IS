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

namespace Fei.Is.Api.Features.Scenes.Queries;

public static class GetScenes
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scenes",
                    async Task<Results<Ok<PagedList<Response>>, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters paramaters
                    ) =>
                    {
                        var query = new Query(user, paramaters);
                        var result = await mediator.Send(query);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetScenes))
                .WithTags(nameof(Scene))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all scenes";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, IValidator<QueryParameters> validator) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var queryParameters = message.Parameters;

            var result = validator.Validate(queryParameters);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var query = context
                .Scenes.AsNoTracking()
                .Where(s => s.OwnerId == message.User.GetUserId())
                .Where(d => d.Name.ToLower().Contains(StringUtils.Normalized(queryParameters.SearchTerm)))
                .Sort(queryParameters.SortBy ?? nameof(Scene.UpdatedAt), queryParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var scenes = await query
                .Paginate(queryParameters)
                .Select(scene => new Response(scene.Id, scene.Name, scene.IsEnabled, scene.CreatedAt, scene.UpdatedAt, scene.LastTriggeredAt))
                .ToListAsync(cancellationToken);

            return Result.Ok(scenes.ToPagedList(totalCount, queryParameters.PageNumber, queryParameters.PageSize));
        }
    }

    public record Response(Guid Id, string Name, bool IsEnabled, DateTime CreatedAt, DateTime UpdatedAt, DateTimeOffset? LastTriggeredAt = null);

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields = [nameof(Scene.Name), nameof(Scene.CreatedAt), nameof(Scene.UpdatedAt)];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
