using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Scenes;

[Collection("IntegrationTests")]
public class DeleteSceneTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
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
