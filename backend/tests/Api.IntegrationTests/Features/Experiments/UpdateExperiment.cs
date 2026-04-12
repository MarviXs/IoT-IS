using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Experiments.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Experiments;

[Collection("IntegrationTests")]
public class UpdateExperimentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateExperiment_ShouldUpdateExperiment()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        var experiment = new ExperimentFake(factory.DefaultUserId).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var job = new JobFake(device.Id).Generate();
        await AppDbContext.Jobs.AddAsync(job);
        await AppDbContext.Experiments.AddAsync(experiment);
        await AppDbContext.SaveChangesAsync();

        var request = new UpdateExperiment.Request(
            "updated",
            null,
            device.Id,
            job.Id,
            DateTime.UtcNow.AddHours(-3),
            DateTime.UtcNow.AddHours(-2)
        );

        var response = await Client.PutAsJsonAsync($"experiments/{experiment.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await AppDbContext.Experiments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == experiment.Id);
        updated!.Note.Should().Be("updated");
        updated.RanJobId.Should().Be(job.Id);
    }
}
