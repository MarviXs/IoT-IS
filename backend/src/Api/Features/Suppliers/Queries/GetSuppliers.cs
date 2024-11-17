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

namespace Fei.Is.Api.Features.Products.Queries;

public static class GetSuppliers
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "suppliers",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetSuppliers))
                .WithTags(nameof(Supplier))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated suppliers";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.Suppliers.AsNoTracking();

            if (message.Parameters.SearchTerm is not null)
            {
                query = query.Where(supplier => supplier.Name != null ? supplier.Name.Contains(message.Parameters.SearchTerm) : false);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pagedQuery = query
                .Sort(message.Parameters.SortBy ?? nameof(Supplier.Name), message.Parameters.Descending)
                .Select((supplier) => new Response(supplier.Id, supplier.Name));

            return pagedQuery.Paginate(message.Parameters).ToPagedList(totalCount, message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(Guid Id, string Name);
}
