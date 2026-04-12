using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Scenes.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Scenes;

[Collection("IntegrationTests")]
public class GetSceneByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetSceneById_ShouldReturnScene()
    {
        var scene = new SceneFake(factory.DefaultUserId).Generate();
        await AppDbContext.Scenes.AddAsync(scene);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"scenes/{scene.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetSceneById.Response>();
        body!.Name.Should().Be(scene.Name);
    }
}
