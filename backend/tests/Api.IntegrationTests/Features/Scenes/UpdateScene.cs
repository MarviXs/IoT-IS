using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Scenes.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Scenes;

[Collection("IntegrationTests")]
public class UpdateSceneTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateScene_ShouldUpdateScene()
    {
        var scene = new SceneFake(factory.DefaultUserId).Generate();
        await AppDbContext.Scenes.AddAsync(scene);
        await AppDbContext.SaveChangesAsync();

        var request = new UpdateScene.Request(
            "updated scene",
            "updated",
            false,
            "{}",
            [new UpdateScene.SceneActionRequest(SceneActionType.NOTIFICATION, null, null, NotificationSeverity.Warning, "warn", null, false)],
            2
        );
        var response = await Client.PutAsJsonAsync($"scenes/{scene.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updated = await AppDbContext.Scenes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == scene.Id);
        updated!.Name.Should().Be("updated scene");
        updated.IsEnabled.Should().BeFalse();
    }
}
