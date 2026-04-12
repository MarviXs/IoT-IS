using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.DeviceCollections.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class GetDeviceCollectionWithSensorsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceCollectionWithSensors_ShouldReturnDeviceAndSensors()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var sensor = new SensorFake(deviceTemplate.Id).Generate();
        var root = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        var device = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate();

        await AppDbContext.Sensors.AddAsync(sensor);
        await AppDbContext.DeviceCollections.AddAsync(root);
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        await AppDbContext.CollectionItems.AddAsync(new Fei.Is.Api.Data.Models.CollectionItem
        {
            CollectionParentId = root.Id,
            DeviceId = device.Id
        });
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"device-collections/{root.Id}/sensors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetDeviceCollectionWithSensors.Response>();
        body.Should().NotBeNull();
        body!.Items.Should().ContainSingle();
        body.Items[0].Id.Should().Be(device.Id);
        body.Items[0].Items.Should().ContainSingle(x => x.Sensor != null && x.Sensor.Id == sensor.Id);
    }
}
