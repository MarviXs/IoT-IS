using System.Linq.Dynamic.Core;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.Features.Jobs.Extensions;

public static class JobExtensions
{
    public static double GetProgress(this Job job)
    {
        if (job == null)
        {
            return 0;
        }

        if (job?.Status == JobStatusEnum.JOB_SUCCEEDED)
        {
            return 100;
        }

        int totalCmds = job!.TotalSteps * job.TotalCycles;
        int completedCmds = (job.CurrentCycle - 1) * job.TotalSteps + (job.CurrentStep - 1);
        double progress = (double)completedCmds / totalCmds * 100;

        return progress;
    }

    //Get current command
    public static string GetCurrentCommand(this Job job)
    {
        if (job == null)
        {
            return "";
        }

        var currentCommand = job.Commands.ElementAtOrDefault(job.CurrentStep - 1)?.Name ?? string.Empty;

        return currentCommand;
    }

    public static IQueryable<Job> GetActiveJobs(this IQueryable<Job> jobs, Guid deviceId)
    {
        return jobs.Where(j => j.DeviceId == deviceId)
            .Where(j => j.Status == JobStatusEnum.JOB_QUEUED || j.Status == JobStatusEnum.JOB_IN_PROGRESS || j.Status == JobStatusEnum.JOB_PAUSED);
    }

    public static IQueryable<Job> GetActiveJobsWithoutQueued(this IQueryable<Job> jobs, Guid deviceId)
    {
        return jobs.Where(j => j.DeviceId == deviceId).Where(j => j.Status == JobStatusEnum.JOB_IN_PROGRESS || j.Status == JobStatusEnum.JOB_PAUSED);
    }
}
