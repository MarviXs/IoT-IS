using Carter;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Queries;

public static class GetNodeSettings
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "system/node-settings",
                    async Task<Ok<Response>> (IMediator mediator, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Query(), cancellationToken);
                        return TypedResults.Ok(result);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(GetNodeSettings))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Get system node settings";
                    o.Description = "Returns current node type, hub connection settings, and configured edge nodes.";
                    return o;
                });
        }
    }

    public record Query() : IRequest<Response>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var settings = await context
                .SystemNodeSettings.AsNoTracking()
                .OrderBy(setting => setting.CreatedAt)
                .Select(setting => new
                {
                    setting.NodeType,
                    setting.HubUrl,
                    setting.HubToken
                })
                .FirstOrDefaultAsync(cancellationToken);

            var edgeNodes = await context
                .EdgeNodes.AsNoTracking()
                .OrderBy(edgeNode => edgeNode.Name)
                .Select(
                    edgeNode => new EdgeNodeResponse(
                        edgeNode.Id,
                        edgeNode.Name,
                        edgeNode.Token,
                        edgeNode.UpdateRateSeconds,
                        edgeNode.CreatedAt,
                        edgeNode.UpdatedAt
                    )
                )
                .ToListAsync(cancellationToken);

            return new Response(
                settings?.NodeType ?? SystemNodeType.Hub,
                settings?.HubUrl,
                settings?.HubToken,
                edgeNodes
            );
        }
    }

    public record Response(
        SystemNodeType NodeType,
        string? HubUrl,
        string? HubToken,
        IReadOnlyCollection<EdgeNodeResponse> EdgeNodes
    );

    public record EdgeNodeResponse(Guid Id, string Name, string Token, int UpdateRateSeconds, DateTime CreatedAt, DateTime UpdatedAt);
}
