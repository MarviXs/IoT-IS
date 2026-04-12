using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Features.JobSchedules.Commands;
using Fei.Is.Api.Features.JobSchedules.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.JobSchedules;

[Collection("IntegrationTests")]
public class JobScheduleEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
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

    [Fact]
    public async Task UpdateJobSchedule_ShouldUpdateSchedule()
    {
        var (device, recipe) = await SeedDeviceWithRecipe();
        var schedule = new JobScheduleFake(device.Id, recipe.Id).Generate();
        await AppDbContext.JobSchedules.AddAsync(schedule);
        await AppDbContext.SaveChangesAsync();

        var request = new UpdateJobSchedule.Request(
            device.Id,
            recipe.Id,
            "updated schedule",
            JobScheduleTypeEnum.Repeat,
            JobScheduleIntervalEnum.Week,
            1,
            DateTimeOffset.UtcNow.AddDays(2),
            null,
            [JobScheduleWeekDayEnum.Monday],
            2,
            false
        );

        var response = await Client.PutAsJsonAsync($"job-schedules/{schedule.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var updated = await AppDbContext.JobSchedules.AsNoTracking().Include(x => x.WeekDays).FirstOrDefaultAsync(x => x.Id == schedule.Id);
        updated!.Name.Should().Be("updated schedule");
        updated.WeekDays.Should().ContainSingle(x => x.Day == JobScheduleWeekDayEnum.Monday);
    }

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
