using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Jobs.Extensions;
using FlatSharp;
using FluentResults;
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

        JobFbs jobFbs =
            new()
            {
                JobId = job.Id.ToString(),
                Commands = job.Commands.Select(c => new CommandFbs { Name = c.Name, Params = c.Params, }).ToList(),
                Status = (JobStatusFbsEnum)job.Status,
                Name = job.Name,
                CurrentStep = job.CurrentStep,
                TotalSteps = job.TotalSteps,
                CurrentCycle = job.CurrentCycle,
                TotalCycles = job.TotalCycles,
                Paused = job.Paused,
                StartedAt = job.StartedAt?.Ticks ?? 0,
                FinishedAt = job.FinishedAt?.Ticks ?? 0,
            };

        int maxSize = JobFbs.Serializer.GetMaxSize(jobFbs);

        byte[] buffer = new byte[maxSize];

        int bytesWritten = JobFbs.Serializer.Write(buffer, jobFbs);

        var result = await mqttClient.PublishAsync(topic, buffer);

        if (result.IsSuccess)
        {
            return Result.Ok();
        }

        return Result.Fail(new MqttError());
    }
}
