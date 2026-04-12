using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.DataPoints.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DataPoints;

[Collection("IntegrationTests")]
public class CreateDataPointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateDataPoints_ShouldReturnNoContent_AndUpdateSampleRate()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var createResponse = await Client.PostAsJsonAsync(
            $"devices/{device.AccessToken}/data",
            new List<CreateDataPoints.Request> { new("samplerate", 12.5, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) }
        );

        createResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await AppDbContext.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == device.Id);
        updated.Should().NotBeNull();
        updated!.SampleRateSeconds.Should().Be(12.5f);
    }
}
