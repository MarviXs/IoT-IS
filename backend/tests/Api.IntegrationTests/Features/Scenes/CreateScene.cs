using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.Scenes.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Scenes;

[Collection("IntegrationTests")]
public class CreateSceneTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateScene_ShouldCreateScene()
    {
        var request = new CreateScene.Request(
            "scene",
            "desc",
            true,
            "{}",
            [new CreateScene.SceneActionRequest(SceneActionType.NOTIFICATION, null, null, NotificationSeverity.Info, "hello", null, false)],
            1
        );

        var response = await Client.PostAsJsonAsync("scenes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var scene = await AppDbContext.Scenes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        scene!.Name.Should().Be("scene");
    }
}
