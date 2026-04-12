using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Auth;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class RefreshTokenTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
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
}
