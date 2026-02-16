using System.Globalization;
using System.Text.Json;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.DataPoints.Queries;
using Fei.Is.Api.Grpc;
using Fei.Is.Api.Redis;
using Fei.Is.Api.Services.DeviceFirmwares;
using Fei.Is.Api.SignalR.Dtos;
using Fei.Is.Api.SignalR.Hubs;
using Fei.Is.Api.SignalR.Interfaces;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Fei.Is.Api.Features.System.Services;

public class EdgeHubSyncGrpcService(
    AppDbContext appContext,
    RedisService redis,
    IHubContext<IsHub, INotificationsClient> hubContext,
    IDeviceFirmwareFileService firmwareFileService
) : EdgeHubSync.EdgeHubSyncBase
{
    private const string EdgeTokenHeader = "x-edge-token";
    private const int DefaultNextSyncSeconds = 5;

    public override async Task<SyncDataPointsResponse> SyncDataPoints(SyncDataPointsRequest request, ServerCallContext context)
    {
        var edgeNode = await AuthorizeAndTouchHeartbeatAsync(context, context.CancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (request.Datapoints.Count == 0)
        {
            return new SyncDataPointsResponse
            {
                NextSyncSeconds = edgeNode.UpdateRateSeconds > 0 ? edgeNode.UpdateRateSeconds : DefaultNextSyncSeconds,
                AcceptedCount = 0,
                SkippedCount = 0
            };
        }

        var candidateIds = request
            .Datapoints.Select(dataPoint => Guid.TryParse(dataPoint.DeviceId, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var existingIds = await appContext
            .Devices.AsNoTracking()
            .Where(device => candidateIds.Contains(device.Id))
            .Select(device => device.Id)
            .ToListAsync(context.CancellationToken);
        var existingIdSet = existingIds.ToHashSet();

        var accepted = 0;
        var skipped = 0;

        foreach (var incoming in request.Datapoints)
        {
            if (!Guid.TryParse(incoming.DeviceId, out var deviceId) || !existingIdSet.Contains(deviceId))
            {
                skipped++;
                continue;
            }

            if (double.IsNaN(incoming.Value) || double.IsInfinity(incoming.Value))
            {
                skipped++;
                continue;
            }

            DateTimeOffset timestamp;
            try
            {
                timestamp = incoming.TimestampUnixMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(incoming.TimestampUnixMs) : now;
            }
            catch (ArgumentOutOfRangeException)
            {
                timestamp = now;
            }

            var deviceIdString = deviceId.ToString();
            var streamEntries = new List<NameValueEntry>
            {
                new("device_id", deviceIdString),
                new("sensor_tag", incoming.SensorTag),
                new("value", incoming.Value.ToString(CultureInfo.InvariantCulture)),
                new("timestamp", timestamp.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture))
            };

            if (incoming.HasLatitude)
            {
                streamEntries.Add(new("latitude", incoming.Latitude.ToString(CultureInfo.InvariantCulture)));
            }
            if (incoming.HasLongitude)
            {
                streamEntries.Add(new("longitude", incoming.Longitude.ToString(CultureInfo.InvariantCulture)));
            }
            if (incoming.HasGridX)
            {
                streamEntries.Add(new("grid_x", incoming.GridX.ToString(CultureInfo.InvariantCulture)));
            }
            if (incoming.HasGridY)
            {
                streamEntries.Add(new("grid_y", incoming.GridY.ToString(CultureInfo.InvariantCulture)));
            }

            await redis.Db.StreamAddAsync("datapoints", streamEntries.ToArray(), maxLength: 500000);
            await redis.Db.StringSetAsync($"device:{deviceIdString}:connected", "1", TimeSpan.FromMinutes(30), flags: CommandFlags.FireAndForget);
            await redis.Db.StringSetAsync(
                $"device:{deviceIdString}:lastSeen",
                now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                flags: CommandFlags.FireAndForget
            );

            var latest = new GetLatestDataPoints.Response(
                timestamp,
                incoming.Value,
                incoming.HasLatitude ? incoming.Latitude : null,
                incoming.HasLongitude ? incoming.Longitude : null,
                incoming.HasGridX ? incoming.GridX : null,
                incoming.HasGridY ? incoming.GridY : null
            );
            await redis.Db.StringSetAsync(
                $"device:{deviceIdString}:{incoming.SensorTag}:last",
                JsonSerializer.Serialize(latest),
                TimeSpan.FromHours(1),
                flags: CommandFlags.FireAndForget
            );

            await hubContext
                .Clients.Group(deviceIdString)
                .ReceiveSensorLastDataPoint(
                    new SensorLastDataPointDto(
                        deviceIdString,
                        incoming.SensorTag,
                        incoming.Value,
                        incoming.HasLatitude ? incoming.Latitude : null,
                        incoming.HasLongitude ? incoming.Longitude : null,
                        incoming.HasGridX ? incoming.GridX : null,
                        incoming.HasGridY ? incoming.GridY : null,
                        timestamp
                    )
                );

            accepted++;
        }

        return new SyncDataPointsResponse
        {
            NextSyncSeconds = edgeNode.UpdateRateSeconds > 0 ? edgeNode.UpdateRateSeconds : DefaultNextSyncSeconds,
            AcceptedCount = accepted,
            SkippedCount = skipped
        };
    }

    public override async Task<GetHubSnapshotResponse> GetHubSnapshot(GetHubSnapshotRequest request, ServerCallContext context)
    {
        await AuthorizeAndTouchHeartbeatAsync(context, context.CancellationToken);

        var templates = await appContext
            .DeviceTemplates.AsNoTracking()
            .Include(template => template.Owner)
            .Include(template => template.Sensors)
            .Include(template => template.Commands)
            .Include(template => template.Recipes)
            .ThenInclude(recipe => recipe.Steps)
            .Include(template => template.Controls)
            .Include(template => template.Firmwares)
            .ToListAsync(context.CancellationToken);

        var devices = await appContext.Devices.AsNoTracking().Include(device => device.Owner).ToListAsync(context.CancellationToken);

        var response = new GetHubSnapshotResponse();

        foreach (var template in templates)
        {
            var templateMessage = new HubTemplate
            {
                Id = template.Id.ToString(),
                OwnerEmail = template.Owner?.Email ?? string.Empty,
                Name = template.Name,
                DeviceType = (int)template.DeviceType,
                IsGlobal = template.IsGlobal,
                EnableMap = template.EnableMap,
                EnableGrid = template.EnableGrid
            };

            if (template.GridRowSpan.HasValue)
            {
                templateMessage.GridRowSpan = template.GridRowSpan.Value;
            }
            if (template.GridColumnSpan.HasValue)
            {
                templateMessage.GridColumnSpan = template.GridColumnSpan.Value;
            }

            templateMessage.Sensors.AddRange(
                template
                    .Sensors.OrderBy(sensor => sensor.Order)
                    .Select(sensor =>
                    {
                        var sensorMessage = new HubSensor
                        {
                            Id = sensor.Id.ToString(),
                            Tag = sensor.Tag,
                            Name = sensor.Name,
                            Order = sensor.Order
                        };

                        if (!string.IsNullOrWhiteSpace(sensor.Unit))
                        {
                            sensorMessage.Unit = sensor.Unit;
                        }
                        if (sensor.AccuracyDecimals.HasValue)
                        {
                            sensorMessage.AccuracyDecimals = sensor.AccuracyDecimals.Value;
                        }
                        if (!string.IsNullOrWhiteSpace(sensor.Group))
                        {
                            sensorMessage.Group = sensor.Group;
                        }

                        return sensorMessage;
                    })
            );

            templateMessage.Commands.AddRange(
                template.Commands.Select(command =>
                {
                    var commandMessage = new HubCommand
                    {
                        Id = command.Id.ToString(),
                        DisplayName = command.DisplayName,
                        Name = command.Name
                    };
                    commandMessage.Params.AddRange(command.Params);
                    return commandMessage;
                })
            );

            templateMessage.Recipes.AddRange(
                template.Recipes.Select(recipe =>
                {
                    var recipeMessage = new HubRecipe { Id = recipe.Id.ToString(), Name = recipe.Name };
                    recipeMessage.Steps.AddRange(
                        recipe
                            .Steps.OrderBy(step => step.Order)
                            .Select(step =>
                            {
                                var stepMessage = new HubRecipeStep
                                {
                                    Id = step.Id.ToString(),
                                    Cycles = step.Cycles,
                                    Order = step.Order
                                };

                                if (step.CommandId.HasValue)
                                {
                                    stepMessage.CommandId = step.CommandId.Value.ToString();
                                }
                                if (step.SubrecipeId.HasValue)
                                {
                                    stepMessage.SubrecipeId = step.SubrecipeId.Value.ToString();
                                }

                                return stepMessage;
                            })
                    );
                    return recipeMessage;
                })
            );

            templateMessage.Controls.AddRange(
                template
                    .Controls.OrderBy(control => control.Order)
                    .Select(control =>
                    {
                        var controlMessage = new HubDeviceControl
                        {
                            Id = control.Id.ToString(),
                            Name = control.Name,
                            Color = control.Color,
                            Type = (int)control.Type,
                            Cycles = control.Cycles,
                            IsInfinite = control.IsInfinite,
                            Order = control.Order
                        };

                        if (control.RecipeId.HasValue)
                        {
                            controlMessage.RecipeId = control.RecipeId.Value.ToString();
                        }
                        if (control.RecipeOnId.HasValue)
                        {
                            controlMessage.RecipeOnId = control.RecipeOnId.Value.ToString();
                        }
                        if (control.RecipeOffId.HasValue)
                        {
                            controlMessage.RecipeOffId = control.RecipeOffId.Value.ToString();
                        }
                        if (control.SensorId.HasValue)
                        {
                            controlMessage.SensorId = control.SensorId.Value.ToString();
                        }

                        return controlMessage;
                    })
            );

            templateMessage.Firmwares.AddRange(
                template.Firmwares.Select(firmware => new HubFirmware
                {
                    Id = firmware.Id.ToString(),
                    VersionNumber = firmware.VersionNumber,
                    IsActive = firmware.IsActive,
                    OriginalFileName = firmware.OriginalFileName,
                    StoredFileName = firmware.StoredFileName
                })
            );

            response.Templates.Add(templateMessage);
        }

        response.Devices.AddRange(
            devices.Select(device =>
            {
                var deviceMessage = new HubDevice
                {
                    Id = device.Id.ToString(),
                    OwnerEmail = device.Owner?.Email ?? string.Empty,
                    Name = device.Name,
                    AccessToken = device.AccessToken,
                    Protocol = (int)device.Protocol
                };

                if (device.DeviceTemplateId.HasValue)
                {
                    deviceMessage.TemplateId = device.DeviceTemplateId.Value.ToString();
                }
                if (device.DataPointRetentionDays.HasValue)
                {
                    deviceMessage.DataPointRetentionDays = device.DataPointRetentionDays.Value;
                }
                if (device.SampleRateSeconds.HasValue)
                {
                    deviceMessage.SampleRateSeconds = device.SampleRateSeconds.Value;
                }
                if (!string.IsNullOrWhiteSpace(device.CurrentFirmwareVersion))
                {
                    deviceMessage.CurrentFirmwareVersion = device.CurrentFirmwareVersion;
                }
                if (!string.IsNullOrWhiteSpace(device.Mac))
                {
                    deviceMessage.Mac = device.Mac;
                }

                return deviceMessage;
            })
        );

        return response;
    }

    public override async Task DownloadFirmware(
        DownloadFirmwareRequest request,
        IServerStreamWriter<FirmwareChunk> responseStream,
        ServerCallContext context
    )
    {
        await AuthorizeAndTouchHeartbeatAsync(context, context.CancellationToken);

        if (!Guid.TryParse(request.TemplateId, out var templateId) || !Guid.TryParse(request.FirmwareId, out var firmwareId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid firmware request"));
        }

        var firmware = await appContext
            .DeviceFirmwares.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == firmwareId && f.DeviceTemplateId == templateId, context.CancellationToken);

        if (firmware == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Firmware not found"));
        }

        await using var stream = await firmwareFileService.OpenReadAsync(firmware.StoredFileName, context.CancellationToken);
        var buffer = new byte[64 * 1024];
        while (true)
        {
            var read = await stream.ReadAsync(buffer, 0, buffer.Length, context.CancellationToken);
            if (read == 0)
            {
                break;
            }

            await responseStream.WriteAsync(new FirmwareChunk { Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, read) });
        }
    }

    private async Task<EdgeNode> AuthorizeAndTouchHeartbeatAsync(ServerCallContext context, CancellationToken cancellationToken)
    {
        var tokenHeader = context.RequestHeaders.FirstOrDefault(header =>
            string.Equals(header.Key, EdgeTokenHeader, StringComparison.OrdinalIgnoreCase)
        );
        var token = tokenHeader?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing edge token"));
        }

        var edgeNode = await appContext.EdgeNodes.AsNoTracking().FirstOrDefaultAsync(node => node.Token == token, cancellationToken);
        if (edgeNode == null)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid edge token"));
        }

        await redis.Db.StringSetAsync(
            $"edge-node:{edgeNode.Id}:last-sync",
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            TimeSpan.FromDays(7),
            flags: CommandFlags.FireAndForget
        );

        return edgeNode;
    }
}
