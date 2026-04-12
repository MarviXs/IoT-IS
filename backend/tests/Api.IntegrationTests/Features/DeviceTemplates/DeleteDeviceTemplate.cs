using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class DeleteDeviceTemplateTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteDeviceTemplate_ShouldDeleteTemplate()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(template).State = EntityState.Detached;

        var response = await Client.DeleteAsync($"device-templates/{template.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AppDbContext.DeviceTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == template.Id)).Should().BeNull();
    }
}
