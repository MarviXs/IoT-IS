using System.Net;
using System.Net.Http.Json;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.DeviceTemplates.Commands;
using Fei.Is.Api.Features.DeviceTemplates.Queries;
using Fei.Is.Api.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using DataCommand = Fei.Is.Api.Data.Models.Command;

namespace Fei.Is.Api.IntegrationTests.Features.DeviceTemplates;

[Collection("IntegrationTests")]
public class ImportDeviceTemplateTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ImportDeviceTemplate_ShouldAcceptLegacyRecipeStepPayload()
    {
        var request = new ImportDeviceTemplate.Request(
            new ImportDeviceTemplate.TemplateRequest(
                "legacy template",
                [new ImportDeviceTemplate.DeviceCommandRequest("Command", "protocolCommand", [1])],
                [],
                Recipes:
                [
                    new ImportDeviceTemplate.RecipeRequest(
                        "recipe",
                        [new ImportDeviceTemplate.RecipeStepRequest("protocolCommand", null, 1, 0)]
                    )
                ],
                DeviceType: DeviceType.Generic
            ),
            "1.0"
        );

        var response = await Client.PostAsJsonAsync("device-templates/import", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ImportDeviceTemplate_ShouldUseFirstCommandWhenLegacyCommandNameIsAmbiguous()
    {
        var request = new ImportDeviceTemplate.Request(
            new ImportDeviceTemplate.TemplateRequest(
                "legacy ambiguous template",
                [
                    new ImportDeviceTemplate.DeviceCommandRequest("First command", "setTube", [1]),
                    new ImportDeviceTemplate.DeviceCommandRequest("Second command", "setTube", [2])
                ],
                [],
                Recipes:
                [
                    new ImportDeviceTemplate.RecipeRequest(
                        "recipe",
                        [new ImportDeviceTemplate.RecipeStepRequest("setTube", null, 1, 0)]
                    )
                ],
                DeviceType: DeviceType.Generic
            ),
            "1.0"
        );

        var response = await Client.PostAsJsonAsync("device-templates/import", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var templateId = await response.Content.ReadFromJsonAsync<Guid>();
        var recipe = await AppDbContext
            .Recipes.AsNoTracking()
            .Include(recipe => recipe.Steps)
            .ThenInclude(step => step.Command)
            .SingleAsync(recipe => recipe.DeviceTemplateId == templateId);

        recipe.Steps.Single().Command!.DisplayName.Should().Be("First command");
        recipe.Steps.Single().Command!.Params.Should().Equal(1);
    }

    [Fact]
    public async Task ImportDeviceTemplate_ShouldResolveDuplicateCommandsByPayloadIdsWithoutSavingThoseIds()
    {
        var firstPayloadCommandId = Guid.NewGuid();
        var secondPayloadCommandId = Guid.NewGuid();
        var request = new ImportDeviceTemplate.Request(
            new ImportDeviceTemplate.TemplateRequest(
                "template with duplicate command names",
                [
                    new ImportDeviceTemplate.DeviceCommandRequest("Same command", "protocolCommand", [1])
                    {
                        Id = firstPayloadCommandId
                    },
                    new ImportDeviceTemplate.DeviceCommandRequest("Same command", "protocolCommand", [2])
                    {
                        Id = secondPayloadCommandId
                    }
                ],
                [],
                Recipes:
                [
                    new ImportDeviceTemplate.RecipeRequest(
                        "recipe",
                        [
                            new ImportDeviceTemplate.RecipeStepRequest("protocolCommand", null, 1, 0)
                            {
                                CommandId = firstPayloadCommandId
                            },
                            new ImportDeviceTemplate.RecipeStepRequest("protocolCommand", null, 1, 1)
                            {
                                CommandId = secondPayloadCommandId
                            }
                        ]
                    )
                ],
                DeviceType: DeviceType.Generic
            ),
            "1.2"
        );

        var response = await Client.PostAsJsonAsync("device-templates/import", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var templateId = await response.Content.ReadFromJsonAsync<Guid>();
        var recipe = await AppDbContext
            .Recipes.AsNoTracking()
            .Include(recipe => recipe.Steps.OrderBy(step => step.Order))
            .ThenInclude(step => step.Command)
            .SingleAsync(recipe => recipe.DeviceTemplateId == templateId);

        recipe.Steps.Select(step => step.CommandId).Should().OnlyHaveUniqueItems();
        recipe.Steps.Select(step => step.CommandId).Should().NotContain(firstPayloadCommandId).And.NotContain(secondPayloadCommandId);
        recipe.Steps.Select(step => step.Command!.DisplayName).Should().Equal("Same command", "Same command");
        recipe.Steps.Select(step => step.Command!.Params.Single()).Should().Equal(1, 2);
    }

    [Fact]
    public async Task ExportDeviceTemplate_ShouldUseCommandIdsForRecipeSteps()
    {
        var template = new DeviceTemplateFake(factory.DefaultUserId).Generate();
        await AppDbContext.DeviceTemplates.AddAsync(template);

        var firstCommand = new DataCommand
        {
            DeviceTemplateId = template.Id,
            DisplayName = "First command",
            Name = "protocolCommand",
            Params = [1]
        };
        var secondCommand = new DataCommand
        {
            DeviceTemplateId = template.Id,
            DisplayName = "Second command",
            Name = "protocolCommand",
            Params = [2]
        };
        await AppDbContext.Commands.AddRangeAsync(firstCommand, secondCommand);

        var recipe = new Recipe { DeviceTemplateId = template.Id, Name = "recipe" };
        await AppDbContext.Recipes.AddAsync(recipe);
        await AppDbContext.SaveChangesAsync();

        await AppDbContext.RecipeSteps.AddRangeAsync(
            new RecipeStep { RecipeId = recipe.Id, CommandId = firstCommand.Id, Cycles = 1, Order = 0 },
            new RecipeStep { RecipeId = recipe.Id, CommandId = secondCommand.Id, Cycles = 1, Order = 1 }
        );
        await AppDbContext.SaveChangesAsync();

        var response = await Client.GetAsync($"device-templates/{template.Id}/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ImportDeviceTemplate.Request>();
        body!.TemplateData.Commands.Select(command => command.Id).Should().BeEquivalentTo([firstCommand.Id, secondCommand.Id]);
        var steps = body!.TemplateData.Recipes!.Single().Steps!;
        steps.Select(step => step.CommandId).Should().Equal(firstCommand.Id, secondCommand.Id);
        steps.Select(step => step.CommandName).Should().OnlyContain(commandName => commandName == null);
        steps.Select(step => step.CommandDisplayName).Should().OnlyContain(commandDisplayName => commandDisplayName == null);
        steps.Select(step => step.CommandParams).Should().OnlyContain(commandParams => commandParams == null);
    }
}
