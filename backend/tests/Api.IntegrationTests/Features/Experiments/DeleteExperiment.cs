using System.Net;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Experiments;

[Collection("IntegrationTests")]
public class DeleteExperimentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
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
