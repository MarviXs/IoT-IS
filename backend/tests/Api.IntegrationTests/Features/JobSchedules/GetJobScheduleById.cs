using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.JobSchedules.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.JobSchedules;

[Collection("IntegrationTests")]
public class GetJobScheduleByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetJobScheduleById_ShouldReturnSchedule()
    {
        var (device, recipe) = await SeedDeviceWithRecipe();
        var schedule = new JobScheduleFake(device.Id, recipe.Id).Generate();
        await AppDbContext.JobSchedules.AddAsync(schedule);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"job-schedules/{schedule.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetJobScheduleById.Response>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(schedule.Id);
    }

    private async Task<(Fei.Is.Api.Data.Models.Device Device, Fei.Is.Api.Data.Models.Recipe Recipe)> SeedDeviceWithRecipe()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        var recipe = new RecipeFake(template.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.Recipes.AddAsync(recipe);
        await AppDbContext.SaveChangesAsync();
        return (device, recipe);
    }
}
