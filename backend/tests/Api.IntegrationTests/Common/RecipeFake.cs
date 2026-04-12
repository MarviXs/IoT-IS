using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class RecipeFake : Faker<Recipe>
{
    public RecipeFake(Guid deviceTemplateId)
    {
        RuleFor(x => x.DeviceTemplateId, _ => deviceTemplateId);
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
    }
}
