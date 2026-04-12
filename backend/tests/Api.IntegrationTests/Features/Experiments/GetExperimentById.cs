using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Experiments.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Experiments;

[Collection("IntegrationTests")]
public class GetExperimentByIdTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
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
}
