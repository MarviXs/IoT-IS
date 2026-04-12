using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.DeviceTemplates.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class CreateDeviceTemplateTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateDeviceTemplate_ShouldCreateTemplate()
    {
        var request = new CreateDeviceTemplate.Request("template a", DeviceType.Generic, false, false, true, 2, 3);

        var response = await Client.PostAsJsonAsync("device-templates", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var template = await AppDbContext.DeviceTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        template!.GridRowSpan.Should().Be(2);
    }
}
