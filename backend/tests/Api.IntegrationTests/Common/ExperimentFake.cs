using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class ExperimentFake : Faker<Experiment>
{
    public ExperimentFake(Guid ownerId)
    {
        CustomInstantiator(f =>
        {
            var startedAt = DateTime.UtcNow.AddHours(-f.Random.Int(2, 12));
            return new Experiment
            {
                OwnerId = ownerId,
                Note = f.Lorem.Sentence(),
                StartedAt = startedAt,
                FinishedAt = startedAt.AddHours(f.Random.Int(1, 6))
            };
        });
    }
}
