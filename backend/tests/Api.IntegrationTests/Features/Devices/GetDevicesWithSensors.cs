using System.Net.Http.Json;
using Fei.Is.Api.Features.Devices.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Devices;

[Collection("IntegrationTests")]
public class GetDevicesWithSensorsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDevicesWithSensors_ShouldReturnAllDevices()
    {
        // Arrange
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var devices = new DeviceFake(factory.DefaultUserId, deviceTemplate.Id).Generate(25);
        await AppDbContext.Devices.AddRangeAsync(devices);
        await AppDbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("devices/sensors-recipes");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<GetDevicesWithSensors.Response>>();
        body.Should().NotBeNull();
        body!.Select(device => device.Id).Should().BeEquivalentTo(devices.Select(device => device.Id));
    }
}
