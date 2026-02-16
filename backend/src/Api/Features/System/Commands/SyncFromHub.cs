using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.System.Services;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Commands;

public static class SyncFromHub
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/edge/sync-from-hub",
                    async Task<Results<Ok<Response>, ValidationProblem>> (
                        IMediator mediator,
                        CancellationToken cancellationToken
                    ) =>
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
                .WithName(nameof(SyncFromHub))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Synchronize edge metadata from hub";
                    o.Description = "For edge nodes, synchronizes templates, devices, and firmware metadata/files from the configured hub.";
                    return o;
                });
        }
    }

    public record Command() : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, EdgeHubSnapshotSyncService syncService) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var settings = await context.SystemNodeSettings.OrderBy(setting => setting.CreatedAt).FirstOrDefaultAsync(cancellationToken);
            if (settings == null || settings.NodeType != SystemNodeType.Edge)
            {
                return Result.Fail(new ValidationError("NodeType", "System node must be configured as edge to sync from hub."));
            }

            if (string.IsNullOrWhiteSpace(settings.HubUrl) || string.IsNullOrWhiteSpace(settings.HubToken))
            {
                return Result.Fail(new ValidationError("HubUrl", "Hub URL and token must be configured."));
            }

            EdgeSnapshotSyncSummary summary;
            try
            {
                summary = await syncService.SyncFromHubAsync(cancellationToken);
            }
            catch (InvalidOperationException exception)
            {
                return Result.Fail(new ValidationError("SyncFromHub", exception.Message));
            }
            catch (Exception exception)
            {
                return Result.Fail(new ValidationError("SyncFromHub", $"Hub synchronization failed: {exception.Message}"));
            }

            return Result.Ok(
                new Response(
                    summary.DevicesCreated,
                    summary.DevicesUpdated,
                    summary.DevicesDeleted,
                    summary.TemplatesCreated,
                    summary.TemplatesUpdated,
                    summary.TemplatesDeleted,
                    summary.SkippedOwnerNotFound,
                    summary.FirmwareFilesDownloaded,
                    summary.UnresolvedTemplateReferences
                )
            );
        }
    }

    public record Response(
        int DevicesCreated,
        int DevicesUpdated,
        int DevicesDeleted,
        int TemplatesCreated,
        int TemplatesUpdated,
        int TemplatesDeleted,
        int SkippedOwnerNotFound,
        int FirmwareFilesDownloaded,
        int UnresolvedTemplateReferences
    );
}
