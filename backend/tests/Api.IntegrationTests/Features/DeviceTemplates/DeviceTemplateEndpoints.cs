using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.DeviceTemplates.Commands;
using Fei.Is.Api.Features.DeviceTemplates.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class DeviceTemplateEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
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
