using System.Net;
using System.Net.Http.Json;
using Bogus;
using Fei.Is.Api.Features.Sensors.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Sensors;

[Collection("IntegrationTests")]
public class UpdateSensorTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateSensor_ShouldReturnOk()
    {
        var deviceTemplate = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(deviceTemplate);
        await AppDbContext.SaveChangesAsync();

        var sensor = new SensorFake(deviceTemplate.Id).Generate();
        await AppDbContext.Sensors.AddAsync(sensor);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(sensor).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

        var request = new UpdateSensorRequestFake().Generate();

        var response = await Client.PutAsJsonAsync($"sensors/{sensor.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await AppDbContext.Sensors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sensor.Id);
        updated.Should().NotBeNull();
        updated!.Tag.Should().Be(request.Tag);
        updated.Name.Should().Be(request.Name);
        updated.Unit.Should().Be(request.Unit);
        updated.AccuracyDecimals.Should().Be(request.AccuracyDecimals);
        updated.Group.Should().Be(request.Group);
    }

    [Fact]
    public async Task UpdateSensor_ShouldReturnNotFound_WhenSensorDoesNotExist()
    {
        var request = new UpdateSensorRequestFake().Generate();

        var response = await Client.PutAsJsonAsync($"sensors/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

public class UpdateSensorRequestFake : Faker<UpdateSensor.Request>
{
    public UpdateSensorRequestFake()
    {
        CustomInstantiator(f =>
            new UpdateSensor.Request(
                f.Random.AlphaNumeric(6),
                f.Commerce.ProductName(),
                f.Random.Bool() ? f.Random.Word() : null,
                f.Random.Bool() ? f.Random.Int(0, 4) : null,
                f.Random.Bool() ? f.Commerce.Department() : null
            )
        );
    }
}
