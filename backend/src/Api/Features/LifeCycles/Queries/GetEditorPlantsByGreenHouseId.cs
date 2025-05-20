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

namespace Fei.Is.Api.Features.GreenHouses.Queries;

public static class GetEditorPlantsByGreenHouseId
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "greenhouses/{greenhouseId}/plants",
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
                .WithName(nameof(GetEditorPlantsByGreenHouseId))
                .WithTags(nameof(Plant))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated plants by GreenHouseId";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid GreenHouseId, QueryParameters Parameters) : IRequest<List<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.EditorPlants.AsNoTracking().Where(p => p.GreenHouseId == message.GreenHouseId);
            // Apply search term if provided
            if (!string.IsNullOrEmpty(message.Parameters.SearchTerm))
            {
                query = query.Where(plant =>
                    plant.Name.Contains(message.Parameters.SearchTerm) || plant.Type.Contains(message.Parameters.SearchTerm)
                );
            }

            var plants = await query.ToListAsync(cancellationToken);

            if (plants == null || plants.Count == 0)
            {
                return new List<Response>(); // Vráti prázdny zoznam, ak nie sú žiadne rastliny
            }

            return plants
                .Select(p => new Response(
                    p.PlantID,
                    p.Name,
                    p.Type,
                    p.Width,
                    p.Height,
                    p.PosX,
                    p.PosY,
                    p.DateCreated,
                    p.CurrentDay,
                    p.Stage,
                    p.CurrentState,
                    p.PlantDetails,
                    p.GreenHouseId
                ))
                .ToList();
        }
    }

    public record Response(
        Guid PlantID,
        string Name,
        string Type,
        int Width,
        int Height,
        int PosX,
        int PosY,
        DateTime DateCreated,
        int CurrentDay,
        string Stage,
        string CurrentState,
        string PlantDetails,
        Guid GreenHouseId
    );
}
