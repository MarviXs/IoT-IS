using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Features.DeviceCollections.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class UpdateDeviceCollectionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateDeviceCollection_ShouldReturnOk()
    {
        var collection = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceCollections.AddAsync(collection);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(collection).State = EntityState.Detached;

        var request = new UpdateDeviceCollectionRequestFake().Generate();

        var response = await Client.PutAsJsonAsync($"device-collections/{collection.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await AppDbContext.DeviceCollections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == collection.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be(request.Name);
    }
}

public class UpdateDeviceCollectionRequestFake : Faker<UpdateDeviceCollection.Request>
{
    public UpdateDeviceCollectionRequestFake()
    {
        CustomInstantiator(f => new UpdateDeviceCollection.Request(f.Commerce.Department()));
    }
}
