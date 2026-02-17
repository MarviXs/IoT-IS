using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Commands;

public static class SyncEdgeNodeNow
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/edge-nodes/{id:guid}/sync-now",
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
                .WithName(nameof(SyncEdgeNodeNow))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Force sync for one edge node";
                    o.Description = "Invalidates node metadata version so the node performs metadata sync on next heartbeat.";
                    return o;
                });
        }
    }

    public record Command(Guid EdgeNodeId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var edgeNodeExists = await context.EdgeNodes.AsNoTracking().AnyAsync(edgeNode => edgeNode.Id == message.EdgeNodeId, cancellationToken);
            if (!edgeNodeExists)
            {
                return Result.Fail(new NotFoundError());
            }

            await redis.Db.KeyDeleteAsync(GetMetadataVersionKey(message.EdgeNodeId), CommandFlags.FireAndForget);
            return Result.Ok();
        }

        private static RedisKey GetMetadataVersionKey(Guid edgeNodeId)
        {
            return $"edge-node:{edgeNodeId}:metadata-version";
        }
    }
}
