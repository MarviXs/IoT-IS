using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class AddDeviceToDeviceCollectionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task AddDeviceToDeviceCollection_ShouldReturnOk()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        var collection = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.DeviceCollections.AddAsync(collection);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.PostAsync($"device-collections/{collection.Id}/devices/{device.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var item = await AppDbContext.CollectionItems.FirstOrDefaultAsync(x => x.CollectionParentId == collection.Id && x.DeviceId == device.Id);
        item.Should().NotBeNull();
    }
}
