using Carter;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Commands;

public static class SyncAllEdgeNodesNow
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/edge-nodes/sync-all-now",
                    async Task<Ok<Response>> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Command(), cancellationToken);
                        return TypedResults.Ok(result);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(SyncAllEdgeNodesNow))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Force sync for all edge nodes";
                    o.Description = "Invalidates metadata version for all configured edge nodes.";
                    return o;
                });
        }
    }

    public record Command() : IRequest<Response>;
    public record Response(int QueuedCount);

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command message, CancellationToken cancellationToken)
        {
            var edgeNodeIds = await context.EdgeNodes.AsNoTracking().Select(edgeNode => edgeNode.Id).ToListAsync(cancellationToken);
            if (edgeNodeIds.Count == 0)
            {
                return new Response(0);
            }

            var keys = edgeNodeIds.Select(edgeNodeId => (RedisKey)$"edge-node:{edgeNodeId}:metadata-version").ToArray();
            await redis.Db.KeyDeleteAsync(keys, CommandFlags.FireAndForget);

            return new Response(edgeNodeIds.Count);
        }
    }
}
