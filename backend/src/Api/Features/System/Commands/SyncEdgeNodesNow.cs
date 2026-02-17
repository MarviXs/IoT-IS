using System.Globalization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.System.Services;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Commands;

public static class SyncEdgeNodeNow
{
    private static readonly TimeSpan SyncRequestTtl = TimeSpan.FromMinutes(10);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/edge-nodes/{id:guid}/sync-now",
                    async Task<Results<NoContent, NotFound, ValidationProblem>> (
                        IMediator mediator,
                        Guid id,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var result = await mediator.Send(new Command(id), cancellationToken);
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(SyncEdgeNodeNow))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Request immediate sync for one edge node";
                    o.Description = "Queues a force sync signal for a specific edge node. It is consumed by the node on next datapoint sync.";
                    return o;
                });
        }
    }

    public record Command(Guid EdgeNodeId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var nodeType = await context
                .SystemNodeSettings.AsNoTracking()
                .OrderBy(setting => setting.CreatedAt)
                .Select(setting => setting.NodeType)
                .FirstOrDefaultAsync(cancellationToken);

            if (nodeType == SystemNodeType.Edge)
            {
                return Result.Fail(new ValidationError("NodeType", "Edge sync-now can only be triggered on hub node."));
            }

            var exists = await context.EdgeNodes.AsNoTracking().AnyAsync(edgeNode => edgeNode.Id == message.EdgeNodeId, cancellationToken);
            if (!exists)
            {
                return Result.Fail(new NotFoundError());
            }

            await redis.Db.StringSetAsync(
                EdgeSyncNowRedisKeys.BuildForceFullSyncKey(message.EdgeNodeId),
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                SyncRequestTtl
            );

            return Result.Ok();
        }
    }
}

public static class SyncAllEdgeNodesNow
{
    private static readonly TimeSpan SyncRequestTtl = TimeSpan.FromMinutes(10);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/edge-nodes/sync-all-now",
                    async Task<Results<Ok<Response>, ValidationProblem>> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Command(), cancellationToken);
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(SyncAllEdgeNodesNow))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Request immediate sync for all edge nodes";
                    o.Description = "Queues force sync signals for all configured edge nodes.";
                    return o;
                });
        }
    }

    public record Command() : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, RedisService redis) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var nodeType = await context
                .SystemNodeSettings.AsNoTracking()
                .OrderBy(setting => setting.CreatedAt)
                .Select(setting => setting.NodeType)
                .FirstOrDefaultAsync(cancellationToken);

            if (nodeType == SystemNodeType.Edge)
            {
                return Result.Fail(new ValidationError("NodeType", "Edge sync-now can only be triggered on hub node."));
            }

            var edgeNodeIds = await context.EdgeNodes.AsNoTracking().Select(edgeNode => edgeNode.Id).ToListAsync(cancellationToken);
            if (edgeNodeIds.Count == 0)
            {
                return Result.Ok(new Response(0));
            }

            var nowUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
            foreach (var edgeNodeId in edgeNodeIds)
            {
                await redis.Db.StringSetAsync(EdgeSyncNowRedisKeys.BuildForceFullSyncKey(edgeNodeId), nowUnixSeconds, SyncRequestTtl);
            }

            return Result.Ok(new Response(edgeNodeIds.Count));
        }
    }

    public record Response(int RequestedEdges);
}
