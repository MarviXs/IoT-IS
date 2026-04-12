using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Auth;

[Collection("IntegrationTests")]
public class RegisterTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_ShouldCreateUser()
    {
        var request = new Fei.Is.Api.Features.Auth.Commands.Register.Endpoint.Request("newuser@test.com", "Password123!");

        var response = await Client.PostAsJsonAsync("auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
