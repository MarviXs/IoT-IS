using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.DeviceFirmwares.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceFirmwares;

[Collection("IntegrationTests")]
public class GetDeviceFirmwaresTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceFirmwares_ShouldReturnFirmwares()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var firmware = new DeviceFirmwareFake(template.Id).Generate();
        await AppDbContext.DeviceFirmwares.AddAsync(firmware);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"device-templates/{template.Id}/firmwares");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<GetDeviceFirmwares.Response>>();
        body.Should().ContainSingle(x => x.Id == firmware.Id);
    }
}
