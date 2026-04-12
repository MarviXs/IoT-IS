using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Scenes.Commands;
using Fei.Is.Api.Features.Scenes.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Scenes;

[Collection("IntegrationTests")]
public class SceneEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateScene_ShouldCreateScene()
    {
        var request = new CreateScene.Request("scene", "desc", true, "{}", [new CreateScene.SceneActionRequest(SceneActionType.NOTIFICATION, null, null, NotificationSeverity.Info, "hello", null, false)], 1);

        var response = await Client.PostAsJsonAsync("scenes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var scene = await AppDbContext.Scenes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        scene!.Name.Should().Be("scene");
    }

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

    [Fact]
    public async Task UpdateScene_ShouldUpdateScene()
    {
        var scene = new SceneFake(factory.DefaultUserId).Generate();
        await AppDbContext.Scenes.AddAsync(scene);
        await AppDbContext.SaveChangesAsync();

        var request = new UpdateScene.Request("updated scene", "updated", false, "{}", [new UpdateScene.SceneActionRequest(SceneActionType.NOTIFICATION, null, null, NotificationSeverity.Warning, "warn", null, false)], 2);
        var response = await Client.PutAsJsonAsync($"scenes/{scene.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updated = await AppDbContext.Scenes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == scene.Id);
        updated!.Name.Should().Be("updated scene");
        updated.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteScene_ShouldDeleteScene()
    {
        var scene = new SceneFake(factory.DefaultUserId).Generate();
        await AppDbContext.Scenes.AddAsync(scene);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(scene).State = EntityState.Detached;

        var response = await Client.DeleteAsync($"scenes/{scene.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AppDbContext.Scenes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == scene.Id)).Should().BeNull();
    }
}
