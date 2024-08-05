using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceCollections.Queries;

public static class GetDeviceCollections
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-collections",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetDeviceCollections))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated device collections";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var queryParameters = message.Parameters;

            var query = context
                .DeviceCollections.AsNoTracking()
                .Where(c => c.OwnerId == message.User.GetUserId())
                .Where(c => c.IsRoot)
                .Where(c => c.Name.ToLower().Contains(StringUtils.Normalized(queryParameters.SearchTerm)))
                .Sort(queryParameters.SortBy ?? nameof(DeviceCollection.UpdatedAt), queryParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var collections = await query.Paginate(queryParameters).Select(c => new Response(c.Id, c.Name)).ToListAsync(cancellationToken);

            return collections.ToPagedList(totalCount, queryParameters.PageNumber, queryParameters.PageSize);
        }
    }

    public record Response(Guid Id, string Name);
}
