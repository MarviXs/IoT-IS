using Fei.Is.Api.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fei.Is.Api.Features.JobSchedules.Services;

public class JobScheduleBootstrapper : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IJobScheduleScheduler _scheduler;
    private readonly ILogger<JobScheduleBootstrapper> _logger;

    public JobScheduleBootstrapper(
        IServiceScopeFactory serviceScopeFactory,
        IJobScheduleScheduler scheduler,
        ILogger<JobScheduleBootstrapper> logger
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var schedules = await context.JobSchedules.AsNoTracking().Include(s => s.WeekDays).ToListAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            try
            {
                await _scheduler.ScheduleAsync(schedule, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule job schedule {ScheduleId} during startup.", schedule.Id);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
