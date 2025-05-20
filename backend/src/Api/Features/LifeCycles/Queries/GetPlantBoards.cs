using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.LifeCycles.Queries;

public static class GetPlantBoards
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "lifeboards",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetPlantBoards))
                .WithTags(nameof(PlantBoard))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated lifecycles";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.PlantBoards.AsNoTracking();

            // Total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            var pagedQuery = query
            //.Sort(message.Parameters.SortBy ?? nameof(Plant.Name), message.Parameters.Descending)
            .Select(board => new Response(board.Id, board.PlantBoardId, board.Rows, board.Cols, board.CreatedAt));

            // Paginate the results
            return pagedQuery.Paginate(message.Parameters).ToPagedList(totalCount, message.Parameters.PageNumber, message.Parameters.PageSize);
        }
    }

    public record Response(Guid Id, Guid plantBoardId, int rows, int cols, DateTime createdAt);
}
