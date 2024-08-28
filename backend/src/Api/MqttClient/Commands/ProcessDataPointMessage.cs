using System.Text;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Redis;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProtoBuf;

namespace Fei.Is.Api.MqttClient.Commands;

public static class ProcessDataPointMessage
{
    [ProtoContract]
    class DataPoint
    {
        [ProtoMember(1)]
        public required string Tag { get; set; }

        [ProtoMember(2)]
        public required double Value { get; set; }

        [ProtoMember(3)]
        public long? Ts { get; set; }
    }

    public record Command(string DeviceAccessToken, ArraySegment<byte> Payload) : IRequest<Result>;

    public sealed class Handler(AppDbContext appContext, RedisService redis) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var redisKey = $"device:{message.DeviceAccessToken}:id";

            // Check if deviceId is in Redis cache
            var cachedDeviceId = await redis.Db.StringGetAsync(redisKey);

            string deviceId;

            if (cachedDeviceId.HasValue)
            {
                deviceId = cachedDeviceId!;
            }
            else
            {
                var deviceGuid = await appContext
                    .Devices.Where(d => d.AccessToken == message.DeviceAccessToken)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (deviceGuid == Guid.Empty)
                {
                    return Result.Fail("Device not found");
                }
                deviceId = deviceGuid.ToString();

                // Store deviceId in Redis cache
                await redis.Db.StringSetAsync(redisKey, deviceId, TimeSpan.FromHours(1));
            }

            redis.Db.StringSet($"device:{deviceId}:connected", "1", TimeSpan.FromMinutes(30));
            redis.Db.StringSet($"device:{deviceId}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            // For testing
            // var base64Decoded = Convert.FromBase64String(Encoding.UTF8.GetString(message.Payload));
            // var datapoint = Serializer.Deserialize<DataPoint>(base64Decoded.AsMemory());

            var datapoint = Serializer.Deserialize<DataPoint>(message.Payload.AsMemory());

            // Push to redis stream
            var redisResult = await redis.Db.StreamAddAsync(
                "datapoints",
                [
                    new("device_id", deviceId.ToString()),
                    new("sensor_tag", datapoint.Tag),
                    new("value", datapoint.Value),
                    new("timestamp", GetDataPointTimeStampOrCurrentTime(datapoint.Ts).ToUnixTimeMilliseconds())
                ],
                maxLength: 500000
            );

            return Result.Ok();
        }

        private static DateTimeOffset GetDataPointTimeStampOrCurrentTime(long? timeStamp)
        {
            return timeStamp.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(timeStamp.Value) : DateTimeOffset.UtcNow;
        }
    }
}
