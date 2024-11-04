using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.ProductCategories.Queries;

public static class GetProductCategories
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "product-categories",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetProductCategories))
                .WithTags(nameof(Category))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get product categories";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context
                .Categories.AsNoTracking()
                .Where(category => category.CategoryName.ToLower().Contains(StringUtils.Normalized(message.Parameters.SearchTerm)))
                .Sort(message.Parameters.SortBy ?? nameof(Category.CategoryName), message.Parameters.Descending)
                .Select((category) => new Response(category.Id, category.CategoryName))
                .Paginate(message.Parameters);

            return query.ToPagedList(query.Count(), message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(Guid Id, string Name);
}
