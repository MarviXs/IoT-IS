using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.DeviceCollections.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class GetDeviceCollectionByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceCollectionById_ShouldReturnNestedItems()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var root = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        var subCollection = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        subCollection.IsRoot = false;
        subCollection.RootCollectionId = root.Id;
        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();

        await AppDbContext.DeviceCollections.AddRangeAsync(root, subCollection);
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        await AppDbContext.CollectionItems.AddRangeAsync(
            new Fei.Is.Api.Data.Models.CollectionItem { CollectionParentId = root.Id, SubCollectionId = subCollection.Id },
            new Fei.Is.Api.Data.Models.CollectionItem { CollectionParentId = subCollection.Id, DeviceId = device.Id }
        );
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"device-collections/{root.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetDeviceCollectionById.Response>();
        body.Should().NotBeNull();
        body!.Items.Should().ContainSingle();
        body.Items[0].Id.Should().Be(subCollection.Id);
        body.Items[0].Items.Should().ContainSingle(x => x.Id == device.Id);
    }

    [Fact]
    public async Task GetDeviceCollectionById_ShouldReturnNotFound_WhenCollectionDoesNotExist()
    {
        var response = await Client.GetAsync($"device-collections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
