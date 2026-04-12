using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Sensors.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Sensors;

[Collection("IntegrationTests")]
public class GetDeviceTemplateSensorsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceTemplateSensors_ShouldReturnOrderedSensors()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var lateSensor = new SensorFake(deviceTemplate.Id).Generate();
        lateSensor.Order = 2;
        var earlySensor = new SensorFake(deviceTemplate.Id).Generate();
        earlySensor.Order = 1;

        await AppDbContext.Sensors.AddRangeAsync(lateSensor, earlySensor);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"device-templates/{deviceTemplate.Id}/sensors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<GetDeviceTemplateSensors.Response>>();
        body.Should().NotBeNull();
        body!.Select(x => x.Id).Should().ContainInOrder(earlySensor.Id, lateSensor.Id);
    }

    [Fact]
    public async Task GetDeviceTemplateSensors_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        var response = await Client.GetAsync($"device-templates/{Guid.NewGuid()}/sensors");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
