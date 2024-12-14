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
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Companies.Queries;

public static class GetCompanies
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "companies",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetCompanies))
                .WithTags(nameof(Company))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated companies";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.Companies.AsNoTracking();

            if (!string.IsNullOrEmpty(message.Parameters.SearchTerm))
            {
                query = query.Where(company =>
                    company.Ic.Contains(message.Parameters.SearchTerm) || company.Title.Contains(message.Parameters.SearchTerm)
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pagedQuery = query
                .Sort(message.Parameters.SortBy ?? nameof(Company.Ic), message.Parameters.Descending)
                .Select((company) => new Response(company.Id, company.Title, company.Ic));

            return pagedQuery.Paginate(message.Parameters).ToPagedList(totalCount, message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(Guid Id, string Title, string Ic);
}
