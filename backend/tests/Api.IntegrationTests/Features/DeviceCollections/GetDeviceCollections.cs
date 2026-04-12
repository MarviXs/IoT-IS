using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.DeviceCollections.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceCollections;

[Collection("IntegrationTests")]
public class GetDeviceCollectionsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceCollections_ShouldReturnOnlyRootCollections()
    {
        var root = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        var child = new DeviceCollectionFake(factory.DefaultUserId).Generate();
        child.IsRoot = false;
        child.RootCollectionId = root.Id;

        await AppDbContext.DeviceCollections.AddRangeAsync(root, child);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync("device-collections");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedList<GetDeviceCollections.Response>>();
        body.Should().NotBeNull();
        body!.Items.Should().ContainSingle(x => x.Id == root.Id);
        body.Items.Should().NotContain(x => x.Id == child.Id);
    }
}
