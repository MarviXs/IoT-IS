using System;
using StackExchange.Redis;

namespace Fei.Is.Api.Redis;

public class RedisService : IDisposable
{
    public IDatabaseAsync Db { get; }
    private readonly IConnectionMultiplexer Connection;

    public RedisService(IConfiguration configuration)
    {
        var host = configuration.GetSection("RedisSettings:Host").Value;
        var port = configuration.GetValue<int?>("RedisSettings:Port") ?? 6379;

        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("Redis host is not configured. Set RedisSettings:Host in configuration or REDIS_HOST in the environment.");
        }

        var config = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Password = configuration.GetSection("RedisSettings:Password").Value
        };

        config.EndPoints.Add(host, port);

        var conn = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(config));
        Connection = conn.Value;

        Db = Connection.GetDatabase(0);
    }

    public void Dispose()
    {
        Connection.Dispose();
    }

    public ISubscriber GetSubscriber()
    {
        return Connection.GetSubscriber();
    }
}
