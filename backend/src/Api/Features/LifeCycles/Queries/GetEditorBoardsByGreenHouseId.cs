using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.GreenHouses.Queries;

public static class GetEditorBoardsByGreenHouseId
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "greenhouses/{greenhouseId:guid}/boards",
                    async Task<Ok<List<Response>>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid greenhouseId,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, greenhouseId, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetEditorBoardsByGreenHouseId))
                .WithTags(nameof(EditorBoard))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get editor boards by GreenHouseId";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid GreenHouseId, QueryParameters Parameters) : IRequest<List<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.EditorPots.AsNoTracking().Where(b => b.GreenHouseId == message.GreenHouseId);

            if (!string.IsNullOrEmpty(message.Parameters.SearchTerm))
            {
                query = query.Where(board =>
                    board.Name.Contains(message.Parameters.SearchTerm) || board.Shape.Contains(message.Parameters.SearchTerm)
                );
            }

            var boards = await query.ToListAsync(cancellationToken);

            return
            [
                .. boards.Select(b => new Response(
                    b.EditorBoardID,
                    b.Name,
                    b.Columns,
                    b.Rows,
                    b.Width,
                    b.Height,
                    b.PosX,
                    b.PosY,
                    b.Shape,
                    b.DateCreated,
                    b.GreenHouseId
                ))
            ];
        }
    }

    public record Response(
        Guid EditorBoardID,
        string Name,
        int Columns,
        int Rows,
        int Width,
        int Height,
        int PosX,
        int PosY,
        string Shape,
        DateTime DateCreated,
        Guid GreenHouseId
    );
}
