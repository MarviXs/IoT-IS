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

namespace Fei.Is.Api.Features.Recipes.Queries;

public static class GetRecipes
{
    public class QueryParameters : SearchParameters
    {
        public Guid? DeviceTemplateId { get; init; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "recipes",
                    async Task<Results<Ok<PagedList<Response>>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, parameters);
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
                .WithName(nameof(GetRecipes))
                .WithTags(nameof(Recipe))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all recipes";
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
                .Recipes.AsNoTracking()
                .Include(d => d.DeviceTemplate)
                .Where(d => d.DeviceTemplate!.OwnerId == message.User.GetUserId())
                .Where(d => d.Name.ToLower().Contains(StringUtils.Normalized(queryParameters.SearchTerm)))
                .Where(d => queryParameters.DeviceTemplateId == null || d.DeviceTemplateId == queryParameters.DeviceTemplateId);

            var totalCount = await query.CountAsync(cancellationToken);

            var recipes = await query
                .Sort(queryParameters.SortBy ?? nameof(Recipe.UpdatedAt), queryParameters.Descending)
                .Paginate(queryParameters)
                .Select(recipe => new Response(recipe.Id, recipe.Name, recipe.UpdatedAt))
                .ToListAsync(cancellationToken);

            return Result.Ok(recipes.ToPagedList(totalCount, queryParameters.PageNumber, queryParameters.PageSize));
        }
    }

    public record Response(Guid Id, string Name, DateTime UpdatedAt);

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields = [nameof(Recipe.Name), nameof(Recipe.CreatedAt), nameof(Recipe.UpdatedAt)];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
