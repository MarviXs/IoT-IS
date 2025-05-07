using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.MqttClient.Publish;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Commands;

public static class PauseJob
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "jobs/{jobId}/pause",
                    async Task<Results<Ok, NotFound, UnprocessableEntity, BadRequest>> (IMediator mediator, ClaimsPrincipal user, Guid jobId) =>
                    {
                        var command = new Command(jobId, user);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<MqttError>())
                        {
                            return TypedResults.UnprocessableEntity();
                        }

                        return TypedResults.Ok();
                    }
                )
                .WithName(nameof(PauseJob))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Pause a job";
                    o.Description = "Pause a job.";
                    return o;
                });
        }
    }

    public record Command(Guid JobId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, PublishJobControl publishJobControl) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var job = await context.Jobs.Include(j => j.Device).FirstOrDefaultAsync(j => j.Id == message.JobId, cancellationToken);
            if (job == null || job.Device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (job.Device.Protocol == DeviceConnectionProtocol.MQTT)
            {
                return await publishJobControl.Execute(job.Device.AccessToken, message.JobId, JobControl.JOB_PAUSE);
            }
            else
            {
                job.Status = JobStatusEnum.JOB_PAUSED;
                context.Jobs.Update(job);
                await context.SaveChangesAsync(cancellationToken);
                return Result.Ok(job.Id);
            }
        }
    }
}
