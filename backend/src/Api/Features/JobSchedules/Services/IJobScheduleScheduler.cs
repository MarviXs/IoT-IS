using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.Features.JobSchedules.Services;

public interface IJobScheduleScheduler
{
    Task ScheduleAsync(JobSchedule schedule, CancellationToken cancellationToken);

    Task UnscheduleAsync(Guid scheduleId, CancellationToken cancellationToken);
}
