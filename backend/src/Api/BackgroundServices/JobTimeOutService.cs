using EFCore.BulkExtensions;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.BackgroundServices;

public class JobTimeOutService(IServiceProvider serviceProvider, ILogger<JobTimeOutService> logger) : BackgroundService
{
    private const int JobTimeout = 7200000;
    private const int ProcessingSpeed = 60000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var timeoutThreshold = DateTime.UtcNow.AddMilliseconds(-JobTimeout);

                var timedOutJobs = await context
                    .Jobs.Where(j => j.Status == JobStatusEnum.JOB_QUEUED && j.CreatedAt < timeoutThreshold)
                    .ToListAsync(stoppingToken);

                foreach (var job in timedOutJobs)
                {
                    job.Status = JobStatusEnum.JOB_TIMED_OUT;
                    job.FinishedAt = DateTime.UtcNow;
                }

                await context.BulkUpdateAsync(timedOutJobs, cancellationToken: stoppingToken);

                if (timedOutJobs.Count != 0)
                {
                    logger.LogInformation("Updated {Count} timed out jobs", timedOutJobs.Count);
                }

                await Task.Delay(ProcessingSpeed, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing job timeout: {ExceptionMessage}", ex.Message);
                await Task.Delay(ProcessingSpeed, stoppingToken);
            }
        }
    }
}
