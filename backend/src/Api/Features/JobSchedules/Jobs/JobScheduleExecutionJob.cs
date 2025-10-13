using System.Linq;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Features.Jobs.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fei.Is.Api.Features.JobSchedules.Jobs;

[DisallowConcurrentExecution]
public class JobScheduleExecutionJob : IJob
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<JobScheduleExecutionJob> _logger;

    public JobScheduleExecutionJob(IServiceScopeFactory serviceScopeFactory, ILogger<JobScheduleExecutionJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!context.MergedJobDataMap.TryGetValue("ScheduleId", out var rawScheduleId) ||
            rawScheduleId is not string scheduleIdValue ||
            !Guid.TryParse(scheduleIdValue, out var scheduleId))
        {
            _logger.LogWarning("Job schedule execution triggered without a valid schedule identifier.");
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jobService = scope.ServiceProvider.GetRequiredService<JobService>();

        var schedule = await dbContext.JobSchedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == scheduleId, context.CancellationToken);

        if (schedule is null)
        {
            _logger.LogWarning("Job schedule {ScheduleId} was not found. Skipping execution.", scheduleId);
            return;
        }

        if (!schedule.IsActive)
        {
            _logger.LogInformation("Job schedule {ScheduleId} is inactive. Skipping execution.", scheduleId);
            return;
        }

        if (schedule.EndTime.HasValue && schedule.EndTime.Value <= DateTimeOffset.UtcNow)
        {
            _logger.LogInformation("Job schedule {ScheduleId} has expired. Skipping execution.", scheduleId);
            return;
        }

        var result = await jobService.CreateJobFromRecipe(schedule.DeviceId, schedule.RecipeId, schedule.Cycles, false, context.CancellationToken);

        if (result.IsFailed)
        {
            _logger.LogError(
                "Failed to create job for schedule {ScheduleId}. Errors: {Errors}",
                scheduleId,
                string.Join(", ", result.Errors.Select(error => error.Message))
            );
            return;
        }

        _logger.LogInformation("Created job {JobId} from schedule {ScheduleId}.", result.Value.Id, scheduleId);
    }
}
