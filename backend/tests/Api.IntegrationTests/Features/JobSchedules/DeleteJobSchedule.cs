using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.JobSchedules;

[Collection("IntegrationTests")]
public class DeleteJobScheduleTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteJobSchedule_ShouldDeleteSchedule()
    {
        var (device, recipe) = await SeedDeviceWithRecipe();
        var schedule = new JobScheduleFake(device.Id, recipe.Id).Generate();
        await AppDbContext.JobSchedules.AddAsync(schedule);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(schedule).State = EntityState.Detached;

        var response = await Client.DeleteAsync($"job-schedules/{schedule.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AppDbContext.JobSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.Id == schedule.Id)).Should().BeNull();
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
