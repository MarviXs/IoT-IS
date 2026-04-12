using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Jobs.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Jobs;

[Collection("IntegrationTests")]
public class GetJobByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetJobById_ShouldReturnJob()
    {
        var job = await SeedJobWithCommand();

        var response = await Client.GetAsync($"jobs/{job.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetJobById.Response>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(job.Id);
        body.Commands.Should().HaveCount(1);
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
