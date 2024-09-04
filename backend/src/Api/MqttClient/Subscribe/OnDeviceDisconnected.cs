using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Jobs.Extensions;
using Fei.Is.Api.Redis;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.MqttClient.Subscribe;

public class OnDeviceDisconnected(AppDbContext context, RedisService redis)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };

    public record ConnectionInfo
    {
        public string? Username { get; init; }
        public long Ts { get; init; }
        public int Sockport { get; init; }
        public string? Reason { get; init; }
        public int ProtoVer { get; init; }
        public string? Proto_name { get; init; }
        public int Keepalive { get; init; }
        public string? Ipaddress { get; init; }
        public int Expiry_interval { get; init; }
        public long Connected_at { get; init; }
        public int Connack { get; init; }
        public string? Clientid { get; init; }
        public bool Clean_start { get; init; }
    }

    private readonly AppDbContext _context = context;
    private readonly RedisService _redis = redis;

    public async Task<Result> Handle(ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
        string jsonString = Encoding.UTF8.GetString(payload.Array, payload.Offset, payload.Count);

        var connectionInfo = JsonSerializer.Deserialize<ConnectionInfo>(jsonString, JsonOptions);

        var deviceAccessToken = connectionInfo.Username;
        var device = await _context.Devices.SingleOrDefaultAsync(d => d.AccessToken == deviceAccessToken, cancellationToken);
        if (device == null)
        {
            return Result.Fail("Device not found");
        }
        await _redis.Db.KeyDeleteAsync($"device:{device.Id}:connected");

        await FailActiveJobs(device.Id, cancellationToken);

        return Result.Ok();
    }

    private async Task FailActiveJobs(Guid deviceId, CancellationToken cancellationToken)
    {
        var activeJobs = await _context.Jobs.GetActiveJobs(deviceId).ToListAsync(cancellationToken);

        foreach (var job in activeJobs)
        {
            job.Status = JobStatusEnum.JOB_FAILED;
            job.FinishedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
