using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.MqttClient.Proto;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProtoBuf;

namespace Fei.Is.Api.MqttClient.Commands;

public static class SendJobStatus
{
    public record Command(Guid DeviceId, string AccessToken, Job? Job = null) : IRequest<Result>;

    public sealed class Handler(MqttClientService mqttClient, AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var job = message.Job;

            if (job == null)
            {
                job = await context
                    .Jobs.AsNoTracking()
                    .Where(j => j.DeviceId == message.DeviceId)
                    .Include(j => j.Commands.OrderBy(c => c.Order))
                    .Where(j => j.Status == JobStatusEnum.QUEUED || j.Status == JobStatusEnum.IN_PROGRESS || j.Status == JobStatusEnum.PAUSED)
                    .FirstOrDefaultAsync(cancellationToken);

                if (job == null)
                {
                    return Result.Fail(new NotFoundError());
                }
            }

            var jobProto = new JobProto
            {
                JobId = job.Id.ToString(),
                Commands = job.Commands.Select(c => new CommandProto { Name = c.Name, Params = c.Params }).ToList(),
                Status = job.Status,
                Name = job.Name,
                CurrentStep = job.CurrentStep,
                TotalSteps = job.TotalSteps,
                CurrentCycle = job.CurrentCycle,
                TotalCycles = job.TotalCycles,
                Paused = job.Paused,
                StartedAt = job.StartedAt,
                FinishedAt = job.FinishedAt
            };

            ArraySegment<byte> bytes;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, jobProto);
                bytes = new ArraySegment<byte>(stream.ToArray());
            }

            var topic = $"devices/{message.AccessToken}/job";

            await mqttClient.PublishAsync(topic, bytes);

            return Result.Ok();
        }
    }
}
