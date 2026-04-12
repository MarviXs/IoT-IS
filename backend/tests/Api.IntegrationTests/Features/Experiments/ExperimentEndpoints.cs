using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.Experiments.Commands;
using Fei.Is.Api.Features.Experiments.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Experiments;

[Collection("IntegrationTests")]
public class ExperimentEndpointsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateExperiment_ShouldCreateManualExperiment()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);
        await AppDbContext.SaveChangesAsync();

        var device = new DeviceFake(factory.DefaultUserId, template.Id).Generate();
        await AppDbContext.Devices.AddAsync(device);
        await AppDbContext.SaveChangesAsync();

        var request = new CreateExperiment.Request(
            "manual note",
            null,
            device.Id,
            false,
            1,
            false,
            DateTime.UtcNow.AddHours(-2),
            DateTime.UtcNow.AddHours(-1)
        );

        var response = await Client.PostAsJsonAsync("experiments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        var experiment = await AppDbContext.Experiments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        experiment.Should().NotBeNull();
        experiment!.Note.Should().Be("manual note");
        experiment.DeviceId.Should().Be(device.Id);
    }

    [Fact]
    public async Task GetExperiments_ShouldReturnExperiments()
    {
        var experiment = new ExperimentFake(factory.DefaultUserId).Generate();
        await AppDbContext.Experiments.AddAsync(experiment);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync("experiments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedList<GetExperiments.Response>>();
        body.Should().NotBeNull();
        body!.Items.Should().Contain(x => x.Id == experiment.Id);
    }

    [Fact]
    public async Task GetExperimentById_ShouldReturnExperiment()
    {
        var experiment = new ExperimentFake(factory.DefaultUserId).Generate();
        await AppDbContext.Experiments.AddAsync(experiment);
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"experiments/{experiment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GetExperimentById.Response>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(experiment.Id);
        body.Note.Should().Be(experiment.Note);
    }

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

        var request = new UpdateExperiment.Request("updated", null, device.Id, job.Id, DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-2));

        var response = await Client.PutAsJsonAsync($"experiments/{experiment.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await AppDbContext.Experiments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == experiment.Id);
        updated!.Note.Should().Be("updated");
        updated.RanJobId.Should().Be(job.Id);
    }

    [Fact]
    public async Task DeleteExperiment_ShouldDeleteExperiment()
    {
        var experiment = new ExperimentFake(factory.DefaultUserId).Generate();
        await AppDbContext.Experiments.AddAsync(experiment);
        await AppDbContext.SaveChangesAsync();
        AppDbContext.Entry(experiment).State = EntityState.Detached;

        var response = await Client.DeleteAsync($"experiments/{experiment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await AppDbContext.Experiments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == experiment.Id)).Should().BeNull();
    }
}
