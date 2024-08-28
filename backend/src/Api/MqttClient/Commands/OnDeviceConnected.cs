using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.MqttClient.Commands;

public static class OnDeviceConnected
{
    public record ConnectionInfo
    {
        public string? Username { get; init; }
        public long Ts { get; init; }
        public int Sockport { get; init; }
        public int Proto_ver { get; init; }
        public string? Proto_name { get; init; }
        public int Keepalive { get; init; }
        public string? Ipaddress { get; init; }
        public int Expiry_interval { get; init; }
        public long Connected_at { get; init; }
        public int Connack { get; init; }
        public string? Clientid { get; init; }
        public bool Clean_start { get; init; }
    }

    public record Command(ArraySegment<byte> Payload) : IRequest<Result>;

    public sealed class Handler(MqttClientService mqttClient, IMediator mediator, AppDbContext context, RedisService redis)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            ArraySegment<byte> payload = message.Payload;

            string jsonString = Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);
            var connectionInfo = JsonSerializer.Deserialize<ConnectionInfo>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } }
            );

            var deviceAccessToken = connectionInfo.Username;

            var device = await context.Devices.SingleOrDefaultAsync(d => d.AccessToken == deviceAccessToken, cancellationToken);
            if (device == null)
            {
                return Result.Fail("Device not found");
            }

            redis.Db.StringSet($"device:{device.Id}:connected", "1", TimeSpan.FromMinutes(30));
            redis.Db.StringSet($"device:{device.Id}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            await mediator.Send(new SendJobStatus.Command(device.Id, deviceAccessToken), cancellationToken);

            return Result.Ok();
        }
    }
}
