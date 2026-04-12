using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Features.Experiments.Commands;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.IntegrationTests.Features.Experiments;

[Collection("IntegrationTests")]
public class CreateExperimentTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
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
}
