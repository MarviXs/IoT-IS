using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class SensorFake : Faker<Sensor>
{
    public SensorFake(Guid deviceTemplateId)
    {
        RuleFor(x => x.DeviceTemplateId, _ => deviceTemplateId);
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Tag, f => f.Random.AlphaNumeric(6));
        RuleFor(x => x.Unit, f => f.Random.Bool() ? f.Random.Word() : null);
        RuleFor(x => x.AccuracyDecimals, f => f.Random.Bool() ? f.Random.Int(0, 4) : null);
        RuleFor(x => x.Order, f => f.Random.Int(0, 10));
        RuleFor(x => x.Group, f => f.Random.Bool() ? f.Commerce.Department() : null);
    }
}
