using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Features.DeviceCollections.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class CreateDeviceCollectionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateDeviceCollection_ShouldReturnCreated()
    {
        var request = new CreateDeviceCollectionRequestFake().Generate();

        var response = await Client.PostAsJsonAsync("device-collections", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<CreateDeviceCollection.Response>();
        created.Should().NotBeNull();

        var collection = await AppDbContext.DeviceCollections.FindAsync(created!.Id);
        collection.Should().NotBeNull();
        collection!.Name.Should().Be(request.Name);
        collection.OwnerId.Should().Be(factory.DefaultUserId);
        collection.IsRoot.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDeviceCollection_ShouldCreateSubCollection_WhenParentExists()
    {
        var parent = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceCollections.AddAsync(parent);
        await AppDbContext.SaveChangesAsync();

        var request = new CreateDeviceCollectionRequestFake().Generate() with
        {
            CollectionParentId = parent.Id
        };

        var response = await Client.PostAsJsonAsync("device-collections", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<CreateDeviceCollection.Response>();
        var collection = await AppDbContext.DeviceCollections.FindAsync(created!.Id);
        collection.Should().NotBeNull();
        collection!.IsRoot.Should().BeFalse();
        collection.RootCollectionId.Should().Be(parent.RootCollectionId);
    }
}

public class CreateDeviceCollectionRequestFake : Faker<CreateDeviceCollection.Request>
{
    public CreateDeviceCollectionRequestFake()
    {
        CustomInstantiator(f => new CreateDeviceCollection.Request(f.Commerce.Department(), null));
    }
}
