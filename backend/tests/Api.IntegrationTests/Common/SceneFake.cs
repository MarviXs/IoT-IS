using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class SceneFake : Faker<Scene>
{
    public SceneFake(Guid ownerId)
    {
        RuleFor(x => x.OwnerId, _ => ownerId);
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Description, f => f.Lorem.Sentence());
        RuleFor(x => x.IsEnabled, _ => true);
        RuleFor(x => x.Condition, _ => null as string);
        RuleFor(x => x.CooldownAfterTriggerTime, _ => 0d);
    }
}
