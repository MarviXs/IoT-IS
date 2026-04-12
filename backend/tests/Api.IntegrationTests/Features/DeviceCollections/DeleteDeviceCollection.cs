using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class DeleteDeviceCollectionTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteDeviceCollection_ShouldReturnNoContent()
    {
        var collection = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceCollections.AddAsync(collection);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(collection).State = EntityState.Detached;

        var response = await Client.DeleteAsync($"device-collections/{collection.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AppDbContext.DeviceCollections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == collection.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteDeviceCollection_ShouldReturnNotFound_WhenCollectionDoesNotExist()
    {
        var response = await Client.DeleteAsync($"device-collections/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
