using StackExchange.Redis;

namespace Fei.Is.DataPointBatchProcessingService.Services;

public class RedisService : IDisposable
{
    public IDatabase Db { get; }
    private readonly IConnectionMultiplexer Connection;
    private readonly string _connectionString;

    public RedisService(IConfiguration Configuration)
    {
        _connectionString = Configuration.GetConnectionString("RedisConnection")!;

        var config = new ConfigurationOptions { AbortOnConnectFail = false, EndPoints = { _connectionString } };

        var conn = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(config));
        Connection = conn.Value;

        Db = Connection.GetDatabase(0);
    }

    public void Dispose()
    {
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
