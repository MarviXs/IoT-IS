using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Auth;
using Fei.Is.Api.Features.Auth.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class AuthEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_ShouldCreateUser()
    {
        var request = new Register.Endpoint.Request("newuser@test.com", "Password123!");

        var response = await Client.PostAsJsonAsync("auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ShouldReturnTokens()
    {
        var request = new Login.Request("admin@test.com", "password");

        var response = await Client.PostAsJsonAsync("auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Login.Response>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnAccessToken()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        var user = await userManager.FindByEmailAsync("admin@test.com");
        var refreshToken = await tokenService.CreateRefreshToken(user!);

        var response = await Client.PostAsJsonAsync("auth/refresh", new Fei.Is.Api.Features.Auth.Commands.RefreshToken.Request(refreshToken));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Fei.Is.Api.Features.Auth.Commands.RefreshToken.Response>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AuthDevice_ShouldAllowKnownDevice()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.PostAsJsonAsync("auth/device", new AuthDevice.Endpoint.Request(device.AccessToken));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthDevice.Response>();
        body!.Result.Should().Be("allow");
        body.Client_attrs.Should().ContainKey("deviceId");
    }
}
