using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.JobSchedules.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.JobSchedules;

[Collection("IntegrationTests")]
public class CreateJobScheduleTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateJobSchedule_ShouldCreateSchedule()
    {
        var (device, recipe) = await SeedDeviceWithRecipe();
        var request = new CreateJobSchedule.Request(recipe.Id, "schedule", JobScheduleTypeEnum.Once, null, null, DateTimeOffset.UtcNow.AddDays(1), null, null, 1, true);

        var response = await Client.PostAsJsonAsync($"devices/{device.Id}/job-schedules", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var schedule = await AppDbContext.JobSchedules.AsNoTracking().Include(x => x.WeekDays).FirstOrDefaultAsync(x => x.Id == id);
        schedule.Should().NotBeNull();
        schedule!.Name.Should().Be("schedule");
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
