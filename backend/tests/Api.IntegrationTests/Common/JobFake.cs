using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class JobFake : Faker<Job>
{
    public JobFake(Guid deviceId)
    {
        CustomInstantiator(f => new Job
        {
            DeviceId = deviceId,
            Name = f.Commerce.ProductName(),
            Status = JobStatusEnum.JOB_QUEUED,
            CurrentStep = 1,
            TotalSteps = 2,
            CurrentCycle = 1,
            TotalCycles = 1,
            Paused = false,
            IsInfinite = false,
            StartedAt = DateTime.UtcNow.AddMinutes(-f.Random.Int(1, 120))
        });
    }
}
