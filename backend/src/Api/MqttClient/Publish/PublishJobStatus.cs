using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Jobs.Extensions;
using FluentResults;
using Google.FlatBuffers;
using JobFlatBuffers;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.MqttClient.Publish;

public class PublishJobStatus(MqttClientService mqttClient, AppDbContext context)
{
    public async Task<Result> Execute(Guid deviceId, string accessToken, Job? job = null)
    {
        if (job == null)
        {
            job = await context.Jobs.AsNoTracking().GetActiveJobs(deviceId).Include(j => j.Commands.OrderBy(c => c.Order)).FirstOrDefaultAsync();

            if (job == null)
            {
                return Result.Fail(new NotFoundError());
            }
        }

        var topic = $"devices/{accessToken}/job";

        var builder = new FlatBufferBuilder(1);
        
        // Create all string offsets first
        var jobIdOffset = builder.CreateString(job.Id.ToString());
        var jobNameOffset = builder.CreateString(job.Name);

        var commandOffsets = new Offset<CommandFbs>[job.Commands.Count];
        for (int i = 0; i < job.Commands.Count; i++)
        {
            var command = job.Commands[i];
            var nameOffset = builder.CreateString(command.Name);
            var paramsOffset = CommandFbs.CreateParamsVector(builder, command.Params.ToArray());
            commandOffsets[i] = CommandFbs.CreateCommandFbs(builder, nameOffset, paramsOffset);
        }
        var commandsOffset = JobFbs.CreateCommandsVector(builder, commandOffsets);

        // Now start building the JobFbs object
        JobFbs.StartJobFbs(builder);
        JobFbs.AddJobId(builder, jobIdOffset);
        JobFbs.AddCommands(builder, commandsOffset);
        JobFbs.AddStatus(builder, (JobStatusFbsEnum)job.Status);
        JobFbs.AddName(builder, jobNameOffset);
        JobFbs.AddCurrentStep(builder, job.CurrentStep);
        JobFbs.AddTotalSteps(builder, job.TotalSteps);
        JobFbs.AddCurrentCycle(builder, job.CurrentCycle);
        JobFbs.AddTotalCycles(builder, job.TotalCycles);
        JobFbs.AddPaused(builder, job.Paused);
        JobFbs.AddStartedAt(builder, job.StartedAt?.Ticks ?? 0);
        JobFbs.AddFinishedAt(builder, job.FinishedAt?.Ticks ?? 0);
        var jobFbs = JobFbs.EndJobFbs(builder);
        builder.Finish(jobFbs.Value);

        var buffer = builder.SizedByteArray();
        var isPublished = await mqttClient.TryPublishAsync(topic, buffer);
        if (isPublished)
        {
            return Result.Ok();
        }

        return Result.Fail(new MqttError());
    }
}
