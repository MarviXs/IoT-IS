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

public class OnDeviceConnected(AppDbContext context, RedisService redis)
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

    public async Task<Result> Handle(ArraySegment<byte> payload, CancellationToken cancellationToken)
    {
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
        redis.Db.StringSetAsync($"device:{device.Id}:connected", "1", TimeSpan.FromMinutes(30));
        redis.Db.StringSetAsync($"device:{device.Id}:lastSeen", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

        await FailActiveJobs(device.Id, cancellationToken);

        return Result.Ok();
    }

    private async Task FailActiveJobs(Guid deviceId, CancellationToken cancellationToken)
    {
        var activeJobs = await context.Jobs.GetActiveJobsWithoutQueued(deviceId).ToListAsync(cancellationToken);

        foreach (var job in activeJobs)
        {
            job.Status = JobStatusEnum.JOB_FAILED;
            job.FinishedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
