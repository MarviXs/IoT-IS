using EFCore.BulkExtensions;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Redis;
using StackExchange.Redis;

namespace Fei.Is.Api.BackgroundServices;

public class StoreDataPointsBatchService(IServiceProvider serviceProvider, ILogger<StoreDataPointsBatchService> logger) : BackgroundService
{
    private const string StreamName = "datapoints";
    private const string GroupName = "store_data";
    private const int ProcessingSpeed = 250;
    private const int MaxPendingTimeUnclaimed = 20000;

    static Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string consumerName = Guid.NewGuid().ToString();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var timescale = scope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();
                var redis = scope.ServiceProvider.GetRequiredService<RedisService>();

                // Create consumer group if it doesn't exist
                if (!await redis.Db.KeyExistsAsync(StreamName) || (await redis.Db.StreamGroupInfoAsync(StreamName)).All(x => x.Name != GroupName))
                {
                    await redis.Db.StreamCreateConsumerGroupAsync(StreamName, GroupName, "0-0", true);
                }

                // Claim pending messages
                var autoClaimResult = await redis.Db.StreamAutoClaimAsync(StreamName, GroupName, consumerName, MaxPendingTimeUnclaimed, "0-0", 5000);
                var messages = await redis.Db.StreamReadGroupAsync(StreamName, GroupName, consumerName, ">");

                messages = [.. messages, .. autoClaimResult.ClaimedEntries];

                if (messages.Length != 0)
                {
                    var dataPoints = new List<DataPoint>();

                    foreach (var message in messages)
                    {
                        try
                        {
                            var parsed = ParseResult(message);

                            if (!double.TryParse(parsed["value"], out var value) || double.IsNaN(value) || double.IsInfinity(value))
                            {
                                continue;
                            }

                            dataPoints.Add(
                                new DataPoint
                                {
                                    DeviceId = Guid.Parse(parsed["device_id"]),
                                    SensorTag = parsed["sensor_tag"],
                                    TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(parsed["timestamp"])),
                                    Value = value
                                }
                            );
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error parsing datapoints message: {MessageId}", message.Id);
                        }
                    }

                    if (dataPoints.Count != 0)
                    {

                        await timescale.BulkInsertAsync(dataPoints, cancellationToken: stoppingToken);
                        var messageIds = messages.Select(m => m.Id).ToArray();
                        await redis.Db.StreamAcknowledgeAsync(StreamName, GroupName, messageIds);
                    }
                }
                await Task.Delay(ProcessingSpeed, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing datapoints messages: {ExceptionMessage}", ex.Message);
                await Task.Delay(ProcessingSpeed, stoppingToken);
            }
        }
    }
}
