using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Features.Experiments.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;

namespace Fei.Is.Api.IntegrationTests.Features.Experiments;

[Collection("IntegrationTests")]
public class GetExperimentsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
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
}
