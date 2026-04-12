using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.Scenes.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Scenes;

[Collection("IntegrationTests")]
public class GetScenesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetScenes_ShouldReturnScenes()
    {
        var scene = new SceneFake(factory.DefaultUserId).Generate();
        await AppDbContext.Scenes.AddAsync(scene);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync("scenes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedList<GetScenes.Response>>();
        body!.Items.Should().Contain(x => x.Id == scene.Id);
    }
}
