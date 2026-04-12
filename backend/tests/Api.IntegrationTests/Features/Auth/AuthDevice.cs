using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class AuthDeviceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task AuthDevice_ShouldAllowKnownDevice()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.PostAsJsonAsync("auth/device", new Fei.Is.Api.Features.Auth.Commands.AuthDevice.Endpoint.Request(device.AccessToken));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Fei.Is.Api.Features.Auth.Commands.AuthDevice.Response>();
        body!.Result.Should().Be("allow");
        body.Client_attrs.Should().ContainKey("deviceId");
    }
}
