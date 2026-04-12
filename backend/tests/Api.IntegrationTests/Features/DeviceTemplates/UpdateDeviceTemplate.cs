using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.DeviceTemplates.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class UpdateDeviceTemplateTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateDeviceTemplate_ShouldUpdateTemplate()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var request = new UpdateDeviceTemplate.Request("updated", DeviceType.Generic, false, false, true, 4, 5);
        var response = await Client.PutAsJsonAsync($"device-templates/{template.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await AppDbContext.DeviceTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == template.Id);
        updated!.Name.Should().Be("updated");
        updated.GridColumnSpan.Should().Be(5);
    }
}
