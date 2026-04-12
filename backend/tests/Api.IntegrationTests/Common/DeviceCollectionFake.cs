using Bogus;
using Fei.Is.Api.Data.Models;

namespace Fei.Is.Api.IntegrationTests.Common;

public class DeviceCollectionFake : Faker<DeviceCollection>
{
    public DeviceCollectionFake(Guid ownerId)
    {
        CustomInstantiator(f =>
        {
            var id = Guid.NewGuid();
            return new DeviceCollection
            {
                Id = id,
                OwnerId = ownerId,
                Name = f.Commerce.Department(),
                IsRoot = true,
                RootCollectionId = id
            };
        });
    }
}
