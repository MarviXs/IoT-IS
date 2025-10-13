using System.Linq;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.JobSchedules.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Fei.Is.Api.Features.JobSchedules.Services;

public class JobScheduleQuartzScheduler : IJobScheduleScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<JobScheduleQuartzScheduler> _logger;

    public JobScheduleQuartzScheduler(ISchedulerFactory schedulerFactory, ILogger<JobScheduleQuartzScheduler> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task ScheduleAsync(JobSchedule schedule, CancellationToken cancellationToken)
    {
        if (!schedule.IsActive)
        {
            await UnscheduleAsync(schedule.Id, cancellationToken);
            _logger.LogInformation("Skipping schedule {ScheduleId} because it is inactive.", schedule.Id);
            return;
        }

        if (schedule.EndTime.HasValue && schedule.EndTime.Value <= DateTimeOffset.UtcNow)
        {
            await UnscheduleAsync(schedule.Id, cancellationToken);
            _logger.LogInformation("Skipping schedule {ScheduleId} because it already expired.", schedule.Id);
            return;
        }

        var scheduler = await _schedulerFactory.GetScheduler();

        var jobDetail = JobBuilder
            .Create<JobScheduleExecutionJob>()
            .WithIdentity(GetJobKey(schedule.Id))
            .UsingJobData("ScheduleId", schedule.Id.ToString())
            .Build();

        var trigger = BuildTrigger(schedule, jobDetail.Key);

        if (trigger is null)
        {
            await UnscheduleAsync(schedule.Id, cancellationToken);
            return;
        }

        await scheduler.DeleteJob(jobDetail.Key, cancellationToken);

        await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);

        _logger.LogInformation("Scheduled job schedule {ScheduleId} with trigger {TriggerKey}.", schedule.Id, trigger.Key);
    }

    public async Task UnscheduleAsync(Guid scheduleId, CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var jobKey = GetJobKey(scheduleId);

        if (await scheduler.CheckExists(jobKey, cancellationToken))
        {
            await scheduler.DeleteJob(jobKey, cancellationToken);
            _logger.LogInformation("Unscheduled job schedule {ScheduleId}.", scheduleId);
        }
    }

    private TriggerBuilder ApplyStartTime(TriggerBuilder builder, JobSchedule schedule, TimeSpan? interval = null)
    {
        var now = DateTimeOffset.UtcNow;

        if (schedule.StartTime > now)
        {
            return builder.StartAt(schedule.StartTime.UtcDateTime);
        }

        if (interval is null)
        {
            return builder.StartNow();
        }

        var elapsed = now - schedule.StartTime;

        if (elapsed <= TimeSpan.Zero)
        {
            return builder.StartNow();
        }

        var intervalTicks = interval.Value.Ticks;

        if (intervalTicks <= 0)
        {
            return builder.StartNow();
        }

        var remainderTicks = elapsed.Ticks % intervalTicks;

        if (remainderTicks == 0)
        {
            return builder.StartNow();
        }

        var next = now + TimeSpan.FromTicks(intervalTicks - remainderTicks);

        return builder.StartAt(next.UtcDateTime);
    }

    private ITrigger? BuildTrigger(JobSchedule schedule, JobKey jobKey)
    {
        var builder = TriggerBuilder
            .Create()
            .WithIdentity(GetTriggerKey(schedule.Id))
            .ForJob(jobKey);

        if (schedule.EndTime.HasValue && schedule.EndTime.Value <= DateTimeOffset.UtcNow)
        {
            _logger.LogInformation("Schedule {ScheduleId} end time is in the past.", schedule.Id);
            return null;
        }

        if (schedule.EndTime.HasValue)
        {
            builder = builder.EndAt(schedule.EndTime.Value.UtcDateTime);
        }

        if (schedule.Type == JobScheduleTypeEnum.Once)
        {
            builder = ApplyStartTime(builder, schedule);
            return builder.WithSimpleSchedule(x => x.WithRepeatCount(0)).Build();
        }

        if (!schedule.Interval.HasValue || !schedule.IntervalValue.HasValue)
        {
            _logger.LogWarning(
                "Schedule {ScheduleId} is missing interval configuration and cannot be scheduled.",
                schedule.Id
            );
            return null;
        }

        return schedule.Interval.Value switch
        {
            JobScheduleIntervalEnum.Second =>
                ApplyStartTime(builder, schedule, TimeSpan.FromSeconds(schedule.IntervalValue.Value))
                    .WithSimpleSchedule(x => x.WithIntervalInSeconds(schedule.IntervalValue.Value).RepeatForever())
                    .Build(),
            JobScheduleIntervalEnum.Minute =>
                ApplyStartTime(builder, schedule, TimeSpan.FromMinutes(schedule.IntervalValue.Value))
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(schedule.IntervalValue.Value).RepeatForever())
                    .Build(),
            JobScheduleIntervalEnum.Hour =>
                ApplyStartTime(builder, schedule, TimeSpan.FromHours(schedule.IntervalValue.Value))
                    .WithSimpleSchedule(x => x.WithIntervalInHours(schedule.IntervalValue.Value).RepeatForever())
                    .Build(),
            JobScheduleIntervalEnum.Day =>
                ApplyStartTime(builder, schedule, TimeSpan.FromDays(schedule.IntervalValue.Value))
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromDays(schedule.IntervalValue.Value)).RepeatForever())
                    .Build(),
            JobScheduleIntervalEnum.Week => BuildWeeklyTrigger(builder, schedule),
            _ => null
        };
    }

    private ITrigger? BuildWeeklyTrigger(TriggerBuilder builder, JobSchedule schedule)
    {
        if (schedule.WeekDays.Count == 0)
        {
            _logger.LogWarning("Weekly schedule {ScheduleId} does not have any days configured.", schedule.Id);
            return null;
        }

        if (schedule.IntervalValue.HasValue && schedule.IntervalValue.Value != 1)
        {
            _logger.LogWarning(
                "Weekly schedule {ScheduleId} interval value {IntervalValue} is not supported. Defaulting to every week.",
                schedule.Id,
                schedule.IntervalValue.Value
            );
        }

        var startTimeUtc = schedule.StartTime.UtcDateTime;
        var days = schedule.WeekDays
            .Select(day => MapWeekDay(day.Day))
            .Distinct()
            .ToArray();

        if (days.Length == 0)
        {
            _logger.LogWarning("Weekly schedule {ScheduleId} resolved with no valid days.", schedule.Id);
            return null;
        }

        var cronExpression = $"{startTimeUtc.Second} {startTimeUtc.Minute} {startTimeUtc.Hour} ? * {string.Join(',', days)}";

        return ApplyStartTime(builder, schedule)
            .WithCronSchedule(cronExpression, cron => cron.InTimeZone(TimeZoneInfo.Utc))
            .Build();
    }

    private static string MapWeekDay(JobScheduleWeekDayEnum day) => day switch
    {
        JobScheduleWeekDayEnum.Sunday => "SUN",
        JobScheduleWeekDayEnum.Monday => "MON",
        JobScheduleWeekDayEnum.Tuesday => "TUE",
        JobScheduleWeekDayEnum.Wednesday => "WED",
        JobScheduleWeekDayEnum.Thursday => "THU",
        JobScheduleWeekDayEnum.Friday => "FRI",
        JobScheduleWeekDayEnum.Saturday => "SAT",
        _ => throw new ArgumentOutOfRangeException(nameof(day), day, null)
    };

    private static JobKey GetJobKey(Guid scheduleId) => new($"job-schedule-{scheduleId}");

    private static TriggerKey GetTriggerKey(Guid scheduleId) => new($"job-schedule-trigger-{scheduleId}");
}
