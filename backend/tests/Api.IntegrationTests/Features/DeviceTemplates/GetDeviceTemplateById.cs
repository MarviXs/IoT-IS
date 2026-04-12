using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.DeviceTemplates.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class GetDeviceTemplateByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetDeviceTemplateById_ShouldReturnTemplate()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"device-templates/{template.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetDeviceTemplateById.Response>();
        body!.Id.Should().Be(template.Id);
    }
}
