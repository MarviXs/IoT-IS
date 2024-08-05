using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceCollections.Queries;

public static class GetDeviceCollectionById
{
    public class QueryParameters
    {
        public int? MaxDepth { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-collections/{collectionId:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid collectionId,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, collectionId, parameters);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetDeviceCollectionById))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a device collection";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid CollectionId, QueryParameters Parameters) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var queryParameters = message.Parameters;

            var deviceCollection = await context
                .DeviceCollections.AsNoTracking()
                .Where(c => c.Id == message.CollectionId)
                .Include(c => c.Items)
                .ThenInclude(i => i.Device)
                .Include(c => c.Items)
                .ThenInclude(i => i.SubCollection)
                .FirstOrDefaultAsync(cancellationToken);

            if (deviceCollection == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var subItems = await LoadSubItemsRecursive(deviceCollection.Items, 0, queryParameters.MaxDepth ?? 10, cancellationToken);
            var response = new Response(deviceCollection.Id, deviceCollection.Name, subItems);

            return Result.Ok(response);
        }

        private async Task<List<SubItemResponse>> LoadSubItemsRecursive(
            IEnumerable<CollectionItem> items,
            int currentDepth,
            int maxDepth,
            CancellationToken cancellationToken
        )
        {
            if (currentDepth >= maxDepth)
                return [];

            var subItemResponses = new List<SubItemResponse>();
            foreach (var item in items)
            {
                if (item.Device != null)
                {
                    subItemResponses.Add(new SubItemResponse(item.Device.Id, item.Device.Name, [], SubItemTypes.Device));
                }
                else if (item.SubCollection != null)
                {
                    // Load the sub-collection items from the context
                    var subCollection = await context
                        .DeviceCollections.Include(c => c.Items)
                        .ThenInclude(i => i.Device)
                        .Include(c => c.Items)
                        .ThenInclude(i => i.SubCollection)
                        .FirstOrDefaultAsync(c => c.Id == item.SubCollectionId, cancellationToken);

                    if (subCollection != null)
                    {
                        var subItems = await LoadSubItemsRecursive(subCollection.Items, currentDepth + 1, maxDepth, cancellationToken);
                        subItemResponses.Add(new SubItemResponse(subCollection.Id, subCollection.Name, subItems, SubItemTypes.SubCollection));
                    }
                }
            }

            return subItemResponses;
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SubItemTypes
    {
        Device = 0,
        SubCollection = 1
    }

    public record SubItemResponse(Guid Id, string Name, List<SubItemResponse> Items, SubItemTypes Type);

    public record Response(Guid Id, string Name, List<SubItemResponse> Items);
}
