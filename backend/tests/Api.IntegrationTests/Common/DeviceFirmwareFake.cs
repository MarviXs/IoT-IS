using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class DeviceFirmwareFake : Faker<DeviceFirmware>
{
    public DeviceFirmwareFake(Guid deviceTemplateId)
    {
        RuleFor(x => x.DeviceTemplateId, _ => deviceTemplateId);
        RuleFor(x => x.VersionNumber, f => $"v{f.Random.Int(1, 9)}.{f.Random.Int(0, 9)}.{f.Random.Int(0, 9)}");
        RuleFor(x => x.IsActive, f => f.Random.Bool());
        RuleFor(x => x.OriginalFileName, f => $"{f.System.FileName()}.bin");
        RuleFor(x => x.StoredFileName, f => $"{Guid.NewGuid()}.bin");
    }
}
