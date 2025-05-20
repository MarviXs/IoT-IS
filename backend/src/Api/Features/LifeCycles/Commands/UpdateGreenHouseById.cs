using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.GreenHouses.Commands;

public static class UpdateGreenHouseById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "greenhouses/{id:guid}",
                    async Task<Results<NoContent, NotFound>> (IMediator mediator, Guid id, Request request) =>
                    {
                        var command = new Command(id, request);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName("UpdateGreenHouseById")
                .WithTags(nameof(GreenHouse))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a greenhouse by Id";
                    return o;
                });
        }
    }

    public record Command(Guid Id, Request Data) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var greenhouse = await context.Greenhouses.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

            if (greenhouse == null)
            {
                return Result.Fail(new NotFoundError());
            }

            // Update fields
            greenhouse.Name = request.Data.Name ?? greenhouse.Name;
            greenhouse.Width = request.Data.Width ?? greenhouse.Width;
            greenhouse.Depth = request.Data.Depth ?? greenhouse.Depth;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public record Request(string? Name, int? Width, int? Depth);
}
