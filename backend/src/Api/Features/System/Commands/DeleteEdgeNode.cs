using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Commands;

public static class DeleteEdgeNode
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "system/edge-nodes/{id:guid}",
                    async Task<Results<NoContent, NotFound>> (IMediator mediator, Guid id, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Command(id), cancellationToken);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(DeleteEdgeNode))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete edge node setting";
                    o.Description = "Removes edge node settings by id.";
                    return o;
                });
        }
    }

    public record Command(Guid EdgeNodeId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var edgeNode = await context.EdgeNodes.FirstOrDefaultAsync(node => node.Id == message.EdgeNodeId, cancellationToken);
            if (edgeNode == null)
            {
                return Result.Fail(new NotFoundError());
            }

            context.EdgeNodes.Remove(edgeNode);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
