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

            app.MapGet(
                    "system/hub-sync/snapshot",
                    async Task<Results<Ok<GetHubSnapshotResponse>, UnauthorizedHttpResult>> (
                        HttpContext httpContext,
                        HubEdgeSyncService service,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var token = httpContext.Request.Headers["x-edge-token"].ToString();
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            return TypedResults.Unauthorized();
                        }

                        var result = await service.GetHubSnapshotAsync(token, cancellationToken);
                        return result is null ? TypedResults.Unauthorized() : TypedResults.Ok(result);
                    }
                )
                .AllowAnonymous()
                .WithName("HubSyncGetSnapshot")
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Get hub metadata snapshot for edge sync";
                    o.Description = "Hub-only internal endpoint returning devices and templates for edge synchronization.";
                    return o;
                });

            app.MapGet(
                    "system/hub-sync/firmwares/{templateId:guid}/{firmwareId:guid}",
                    async Task<IResult> (
                        HttpContext httpContext,
                        Guid templateId,
                        Guid firmwareId,
                        HubEdgeSyncService service,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var token = httpContext.Request.Headers["x-edge-token"].ToString();
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            return TypedResults.Unauthorized();
                        }

                        var (status, stream, fileName) = await service.DownloadFirmwareAsync(templateId, firmwareId, token, cancellationToken);
                        if (status == HubEdgeFirmwareDownloadStatus.Unauthorized)
                        {
                            return TypedResults.Unauthorized();
                        }

                        if (status == HubEdgeFirmwareDownloadStatus.NotFound || stream is null)
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.File(stream, contentType: "application/octet-stream", fileDownloadName: fileName, enableRangeProcessing: false);
                    }
                )
                .AllowAnonymous()
                .WithName("HubSyncDownloadFirmware")
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Download firmware binary for edge sync";
                    o.Description = "Hub-only internal endpoint for downloading firmware binary data.";
                    return o;
                });
        }
    }
}
