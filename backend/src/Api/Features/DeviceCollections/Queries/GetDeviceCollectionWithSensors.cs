using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceCollections.Queries;

public static class GetDeviceCollectionWithSensors
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-collections/{collectionId:guid}/sensors",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid collectionId) =>
                    {
                        var query = new Query(user, collectionId);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetDeviceCollectionWithSensors))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a device collection with sensors";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid CollectionId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var deviceCollection = await context
                .DeviceCollections.AsNoTracking()
                .Where(c => c.Id == message.CollectionId)
                .Include(c => c.Items)
                .ThenInclude(i => i.Device)
                .ThenInclude(d => d!.DeviceTemplate)
                .ThenInclude(dt => dt!.Sensors)
                .Include(c => c.Items)
                .ThenInclude(i => i.SubCollection)
                .FirstOrDefaultAsync(cancellationToken);

            if (deviceCollection == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var subItems = await LoadSubItemsRecursive(deviceCollection.Items, 0, 20, cancellationToken);
            var response = new Response(deviceCollection.Id, deviceCollection.Name, subItems, SubItemTypes.SubCollection, null);

            return Result.Ok(response);
        }

        private async Task<List<Response>> LoadSubItemsRecursive(
            IEnumerable<CollectionItem> items,
            int currentDepth,
            int maxDepth,
            CancellationToken cancellationToken
        )
        {
            if (currentDepth >= maxDepth)
                return [];

            var subItemResponses = new List<Response>();
            foreach (var item in items)
            {
                if (item.Device != null)
                {
                    var sensorItems =
                        item.Device.DeviceTemplate?.Sensors.Select(s => new Response(
                                s.Id,
                                s.Name,
                                [],
                                SubItemTypes.Sensor,
                                new SensorResponse(s.Id, s.Name, s.Unit, s.Tag, s.AccuracyDecimals)
                            ))
                            .ToList() ?? [];

                    var deviceResponse = new Response(item.Device.Id, item.Device.Name, sensorItems, SubItemTypes.Device, null);
                    subItemResponses.Add(deviceResponse);
                }
                else if (item.SubCollection != null)
                {
                    // Load the sub-collection items from the context
                    var subCollection = await context
                        .DeviceCollections.Include(c => c.Items)
                        .ThenInclude(i => i.Device)
                        .ThenInclude(d => d!.DeviceTemplate)
                        .ThenInclude(dt => dt!.Sensors)
                        .Include(c => c.Items)
                        .ThenInclude(i => i.SubCollection)
                        .FirstOrDefaultAsync(c => c.Id == item.SubCollectionId, cancellationToken);

                    if (subCollection != null)
                    {
                        var subItems = await LoadSubItemsRecursive(subCollection.Items, currentDepth + 1, maxDepth, cancellationToken);
                        subItemResponses.Add(new Response(subCollection.Id, subCollection.Name, subItems, SubItemTypes.SubCollection, null));
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
        SubCollection = 1,
        Sensor = 2
    }

    public record SensorResponse(Guid Id, string Name, string Unit, string Tag, int? AccuracyDecimals);

    public record Response(Guid Id, string Name, List<Response> Items, SubItemTypes Type, SensorResponse? Sensor);
}
