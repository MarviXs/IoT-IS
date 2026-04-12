using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Auth.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class LoginTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
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
}
