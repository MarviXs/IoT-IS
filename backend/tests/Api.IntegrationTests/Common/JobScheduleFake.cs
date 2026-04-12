using Bogus;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class JobScheduleFake : Faker<JobSchedule>
{
    public JobScheduleFake(Guid deviceId, Guid recipeId)
    {
        CustomInstantiator(f => new JobSchedule
        {
            DeviceId = deviceId,
            RecipeId = recipeId,
            Name = f.Commerce.ProductName(),
            Type = JobScheduleTypeEnum.Once,
            Interval = null,
            IntervalValue = null,
            StartTime = DateTimeOffset.UtcNow.AddDays(1),
            EndTime = null,
            Cycles = 1,
            IsActive = true
        });
    }
}
