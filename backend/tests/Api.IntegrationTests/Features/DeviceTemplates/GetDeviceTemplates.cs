using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.DeviceTemplates.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class GetDeviceTemplatesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceTemplates_ShouldReturnTemplates()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync("device-templates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedList<GetDeviceTemplates.Response>>();
        body!.Items.Should().Contain(x => x.Id == template.Id);
    }
}
