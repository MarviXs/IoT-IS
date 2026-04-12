using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Sensors;

[Collection("IntegrationTests")]
public class DeleteSensorTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteSensor_ShouldReturnNoContent()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var sensor = new SensorFake(deviceTemplate.Id).Generate();
        await AppDbContext.Sensors.AddAsync(sensor);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(sensor).State = EntityState.Detached;

        var response = await Client.DeleteAsync($"sensors/{sensor.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AppDbContext.Sensors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sensor.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteSensor_ShouldReturnNotFound_WhenSensorDoesNotExist()
    {
        var response = await Client.DeleteAsync($"sensors/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
