using System.Linq.Dynamic.Core;
using Fei.Is.Api.Common.Utils;
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

        if (job?.Status == JobStatusEnum.JOB_DONE)
        {
            return 100;
        }

        int totalCmds = job!.TotalSteps * job.TotalCycles;
        int completedCmds = (job.CurrentCycle - 1) * job.TotalSteps + (job.CurrentStep - 1);
        double progress = (double)completedCmds / totalCmds * 100;

        return progress;
    }
}