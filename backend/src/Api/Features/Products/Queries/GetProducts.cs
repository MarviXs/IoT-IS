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

public static class GetProducts
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "products",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetProducts))
                .WithTags(nameof(Product))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated products";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.Products.AsNoTracking();

            if (!string.IsNullOrEmpty(message.Parameters.SearchTerm))
            {
                query = query.Where(product =>
                    product.PLUCode.Contains(message.Parameters.SearchTerm)
                    || (product.Code != null ? product.Code.Contains(message.Parameters.SearchTerm) : false)
                    || (product.CzechName != null ? product.CzechName.Contains(message.Parameters.SearchTerm) : false)
                );
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pagedQuery = query
                .Sort(message.Parameters.SortBy ?? nameof(Product.PLUCode), message.Parameters.Descending)
                .Select((product) => new Response(product.Id, product.Code,product.LatinName, product.CzechName, product.PLUCode, product.Category.Id, product.Supplier.Id));

            return pagedQuery.Paginate(message.Parameters).ToPagedList(totalCount, message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(Guid Id, string? code, string LatinName, string? CzechName, string pluCode, Guid categoryId, Guid supplierId);
}
