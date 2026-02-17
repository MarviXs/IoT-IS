using Carter;
using Fei.Is.Api.Features.System.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.System.Commands;

public static class HubSyncApi
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/hub-sync/datapoints",
                    async Task<Results<Ok<SyncDataPointsResponse>, UnauthorizedHttpResult>> (
                        HttpContext httpContext,
                        SyncDataPointsRequest request,
                        HubEdgeSyncService service,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var token = httpContext.Request.Headers["x-edge-token"].ToString();
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            return TypedResults.Unauthorized();
                        }

                        var result = await service.SyncDataPointsAsync(request, token, cancellationToken);
                        return result is null ? TypedResults.Unauthorized() : TypedResults.Ok(result);
                    }
                )
                .AllowAnonymous()
                .WithName("HubSyncDataPoints")
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Accept edge datapoint sync payload";
                    o.Description = "Hub-only internal endpoint for edge datapoint sync with token authentication.";
                    return o;
                });
        }
    }
}
