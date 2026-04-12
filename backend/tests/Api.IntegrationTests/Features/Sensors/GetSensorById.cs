using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Sensors.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Sensors;

[Collection("IntegrationTests")]
public class GetSensorByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetSensorById_ShouldReturnSensor()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var sensor = new SensorFake(deviceTemplate.Id).Generate();
        await AppDbContext.Sensors.AddAsync(sensor);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"sensors/{sensor.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetSensorById.Response>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(sensor.Id);
        body.Name.Should().Be(sensor.Name);
        body.Tag.Should().Be(sensor.Tag);
        body.Unit.Should().Be(sensor.Unit);
    }

    [Fact]
    public async Task GetSensorById_ShouldReturnNotFound_WhenSensorDoesNotExist()
    {
        var response = await Client.GetAsync($"sensors/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
