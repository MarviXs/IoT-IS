using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.GreenHouses.Queries;

public static class GetGreenhouseById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "greenhouses/{id:Guid}",
                    async Task<Results<Ok<Response>, NotFound>> (AppDbContext context, Guid id) =>
                    {
                        var greenhouse = await context.Greenhouses.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);

                        if (greenhouse is null)
                        {
                            return TypedResults.NotFound();
                        }

                        var response = new Response(
                            Id: greenhouse.Id,
                            GreenHouseID: greenhouse.GreenHouseID,
                            Name: greenhouse.Name,
                            Width: greenhouse.Width,
                            Depth: greenhouse.Depth,
                            DateCreated: greenhouse.DateCreated
                        );

                        return TypedResults.Ok(response);
                    }
                )
                .WithName("GetGreenhouseById")
                .WithTags(nameof(GreenHouse))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get greenhouse by ID";
                    return o;
                });
        }
    }

    public record Query(Guid Id) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var greenhouse = await context.Greenhouses.AsNoTracking().FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (greenhouse is null)
            {
                return Result.Fail(new NotFoundError());
            }

            var response = new Response(
                Id: greenhouse.Id,
                GreenHouseID: greenhouse.GreenHouseID,
                Name: greenhouse.Name,
                Width: greenhouse.Width,
                Depth: greenhouse.Depth,
                DateCreated: greenhouse.DateCreated
            );

            return Result.Ok(response);
        }
    }

    public record Response(Guid Id, Guid GreenHouseID, string Name, int Width, int Depth, DateTime DateCreated);
}
