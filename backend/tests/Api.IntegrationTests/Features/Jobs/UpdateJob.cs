using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Jobs.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Jobs;

[Collection("IntegrationTests")]
public class UpdateJobTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateJob_ShouldUpdateJob()
    {
        var job = await SeedJobWithCommand();
        var request = new UpdateJob.Request("updated job", 1, 2, 1, 1, true, JobStatusEnum.JOB_IN_PROGRESS);

        var response = await Client.PutAsJsonAsync($"jobs/{job.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await AppDbContext.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == job.Id);
        updated!.Name.Should().Be("updated job");
        updated.Paused.Should().BeTrue();
        updated.Status.Should().Be(JobStatusEnum.JOB_IN_PROGRESS);
    }

    private async Task<Job> SeedJobWithCommand()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var job = new JobFake(device.Id).Generate();
        await AppDbContext.Jobs.AddAsync(job);
        await AppDbContext.SaveChangesAsync();

        await AppDbContext.JobCommands.AddAsync(new JobCommand
        {
            JobId = job.Id,
            Order = 1,
            OriginalCommandId = Guid.NewGuid(),
            DisplayName = "Display",
            Name = "command_1",
            Params = [1.2]
        });
        await AppDbContext.SaveChangesAsync();

        return job;
    }
}
