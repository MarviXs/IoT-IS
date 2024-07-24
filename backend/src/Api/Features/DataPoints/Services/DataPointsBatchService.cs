using System.Collections.Concurrent;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.Features.DataPoints.Services;

public class DataPointsBatchService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly ConcurrentQueue<DataPoint> _dataPointQueue = new();
    private const int FlushInterval = 200;
    private const int MaxQueueSize = 5000;

    public void Enqueue(DataPoint dataPoint)
    {
        _dataPointQueue.Enqueue(dataPoint);
        if (_dataPointQueue.Count >= MaxQueueSize)
        {
            FlushQueueAsync(CancellationToken.None).Wait();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(FlushInterval, stoppingToken);

            if (!_dataPointQueue.IsEmpty)
            {
                await FlushQueueAsync(stoppingToken);
            }
        }
    }

    private async Task FlushQueueAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var timescaleContext = scope.ServiceProvider.GetRequiredService<TimeScaleDbContext>();

        var batch = new List<DataPoint>();
        while (_dataPointQueue.TryDequeue(out var dataPoint))
        {
            batch.Add(dataPoint);
        }

        if (batch.Count > 0)
        {
            await timescaleContext.BulkInsertAsync(batch, cancellationToken);
        }
    }
}
