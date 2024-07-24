using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DataPoints.Services;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Commands;

public static class UpdateJobStatus
{
    public record DataPointRequest(string Tag, double Value);

    public record Request(
        Guid Uid,
        JobStatusEnum RetCode,
        JobStatusEnum Code,
        int CurrentStep,
        int TotalSteps,
        int CurrentCycle,
        List<DataPointRequest> Data
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "devices/{deviceId:guid}/jobs/{jobId:guid}/status",
                    async Task<Results<NoContent, NotFound, ValidationProblem>> (IMediator mediator, Request request, Guid deviceId, Guid jobId) =>
                    {
                        var command = new Command(request, deviceId, jobId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(UpdateJobStatus))
                .WithTags(nameof(JobStatus))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update job status";
                    o.Description = "This endpoint is called by the device to update the status of a job.";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid DeviceId, Guid JobId) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext appContext, DataPointsBatchService dataPointsBatchService, IValidator<Command> validator)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(message, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var device = await appContext.Devices.AsNoTracking().FirstOrDefaultAsync(device => device.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var job = await appContext.Jobs.Include(job => job.Status).FirstOrDefaultAsync(job => job.Id == message.JobId, cancellationToken);
            if (job == null || job.DeviceId != message.DeviceId)
            {
                return Result.Fail(new NotFoundError());
            }

            await UpdateOrCreateJobStatusAsync(job, message.Request, cancellationToken);

            SaveDataPoints(device.Id, message.Request.Data);

            await appContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(message.JobId);
        }

        private async Task UpdateOrCreateJobStatusAsync(Job job, Request request, CancellationToken cancellationToken)
        {
            if (job.Status == null)
            {
                var jobStatus = new JobStatus
                {
                    RetCode = request.RetCode,
                    Code = request.Code,
                    CurrentStep = request.CurrentStep,
                    TotalSteps = request.TotalSteps,
                    CurrentCycle = request.CurrentCycle,
                    JobId = job.Id,
                };

                await appContext.JobStatuses.AddAsync(jobStatus, cancellationToken);
            }
            else
            {
                job.Status.RetCode = request.RetCode;
                job.Status.Code = request.Code;
                job.Status.CurrentStep = request.CurrentStep;
                job.Status.TotalSteps = request.TotalSteps;
                job.Status.CurrentCycle = request.CurrentCycle;
            }
        }

        private void SaveDataPoints(Guid deviceId, List<DataPointRequest> dataPoints)
        {
            var dataPointsEntities = dataPoints.Select(
                dataPoint =>
                    new DataPoint
                    {
                        DeviceId = deviceId,
                        SensorTag = dataPoint.Tag,
                        TimeStamp = DateTimeOffset.UtcNow,
                        Value = dataPoint.Value
                    }
            );

            foreach (var dataPoint in dataPointsEntities)
            {
                dataPointsBatchService.Enqueue(dataPoint);
            }
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.Uid).NotEmpty();
            RuleFor(command => command.Request.RetCode).IsInEnum();
            RuleFor(command => command.Request.Code).IsInEnum();
            RuleFor(command => command.Request.CurrentStep).GreaterThanOrEqualTo(0);
            RuleFor(command => command.Request.TotalSteps).GreaterThanOrEqualTo(0);
            RuleFor(command => command.Request.CurrentCycle).GreaterThanOrEqualTo(0);
        }
    }
}
