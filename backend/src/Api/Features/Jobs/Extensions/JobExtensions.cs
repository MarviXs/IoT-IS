using System.Linq.Dynamic.Core;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.Extensions;

public static class JobExtensions
{
    public static double GetProgress(this Job job)
    {
        if (job.Status == null)
        {
            return 0;
        }

        if (job.Status?.Code == JobStatusEnum.JOB_DONE)
        {
            return 100;
        }

        int totalCmds = job.NoOfCmds * job.NoOfReps;
        int completedCmds = ((job.Status?.CurrentCycle - 1) ?? 0) * job.NoOfCmds + ((job.Status?.CurrentStep - 1) ?? 0);
        double progress = (double)completedCmds / totalCmds * 100;

        return progress;
    }
}
