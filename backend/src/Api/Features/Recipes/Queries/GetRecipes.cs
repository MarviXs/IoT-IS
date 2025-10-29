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
        public Guid? DeviceId { get; init; }
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
            var qp = message.Parameters;

            var val = validator.Validate(qp);
            if (!val.IsValid)
                return Result.Fail(new ValidationError(val));

            // Admins can access all templates; other users are constrained to owned/shared templates.
            IQueryable<Guid> accessibleTemplateIds;
            if (message.User.IsAdmin())
            {
                accessibleTemplateIds = context.DeviceTemplates.Select(t => t.Id);
            }
            else
            {
                var userId = message.User.GetUserId();

                // 1) All templates the user can access: owned OR via shared devices.
                //    (If you support template-level shares, UNION them here.)
                accessibleTemplateIds = context
                    .DeviceTemplates.Where(t => t.OwnerId == userId)
                    .Select(t => t.Id)
                    .Union(
                        context
                            .Devices.Where(
                                d =>
                                    d.DeviceTemplateId != null
                                    && d.SharedWithUsers.Any(s => s.SharedToUserId == userId)
                            )
                            .Select(d => d.DeviceTemplateId!.Value)
                    );
            }

            // 2) Base query: recipes under accessible templates only.
            var query = context.Recipes.AsNoTracking().Where(r => accessibleTemplateIds.Contains(r.DeviceTemplateId));

            // 3) Search
            if (!string.IsNullOrWhiteSpace(qp.SearchTerm))
            {
                // If using Npgsql, prefer ILIKE for case/diacritic friendly search:
                // query = query.Where(r => EF.Functions.ILike(r.Name, $"%{qp.SearchTerm}%"));
                query = query.Where(r => r.Name.ToLower().Contains(StringUtils.Normalized(qp.SearchTerm)));
            }

            // 4) Optional filters
            if (qp.DeviceTemplateId is Guid templateId)
            {
                query = query.Where(r => r.DeviceTemplateId == templateId);
            }

            if (qp.DeviceId is Guid deviceId)
            {
                // Constrain to the template that the given device belongs to
                var templateIdForDevice = context.Devices.Where(d => d.Id == deviceId).Select(d => d.DeviceTemplateId);

                query = query.Where(r => templateIdForDevice.Contains(r.DeviceTemplateId));
            }

            // 5) Count, sort, paginate, project
            var totalCount = await query.CountAsync(cancellationToken);

            var recipes = await query
                .Sort(qp.SortBy ?? nameof(Recipe.UpdatedAt), qp.Descending)
                .Paginate(qp)
                .Select(r => new Response(r.Id, r.Name, r.UpdatedAt))
                .ToListAsync(cancellationToken);

            return Result.Ok(recipes.ToPagedList(totalCount, qp.PageNumber, qp.PageSize));
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
