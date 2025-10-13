using System.Linq;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.JobSchedules.Commands;

public static class UpdateJobSchedule
{
    public record Request(
        Guid DeviceId,
        Guid RecipeId,
        string Name,
        JobScheduleTypeEnum Type,
        JobScheduleIntervalEnum? Interval,
        int? IntervalValue,
        DateTimeOffset StartTime,
        DateTimeOffset? EndTime,
        List<JobScheduleWeekDayEnum>? DaysOfWeek,
        int Cycles,
        bool IsActive
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "job-schedules/{scheduleId:guid}",
                    async Task<Results<Created<Guid>, NotFound, ForbidHttpResult, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid scheduleId
                    ) =>
                    {
                        var command = new Command(scheduleId, user, request);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created($"job-schedules/{result.Value}", result.Value);
                    }
                )
                .WithName(nameof(UpdateJobSchedule))
                .WithTags(nameof(JobSchedule))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a job schedule";
                    return o;
                });
        }
    }

    public record Command(Guid ScheduleId, ClaimsPrincipal User, Request Request) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var schedule = await context
                .JobSchedules.Include(s => s.WeekDays)
                .Include(s => s.Device)
                .ThenInclude(d => d!.SharedWithUsers)
                .FirstOrDefaultAsync(s => s.Id == message.ScheduleId, cancellationToken);
            if (schedule == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var currentDevice = schedule.Device;
            if (currentDevice == null || !currentDevice.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            if (schedule.DeviceId != message.Request.DeviceId)
            {
                var targetDevice = await context
                    .Devices.Include(d => d.SharedWithUsers)
                    .FirstOrDefaultAsync(d => d.Id == message.Request.DeviceId, cancellationToken);
                if (targetDevice == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                if (!targetDevice.CanEdit(message.User))
                {
                    return Result.Fail(new ForbiddenError());
                }

                schedule.DeviceId = targetDevice.Id;
                schedule.Device = targetDevice;
            }

            var recipeExists = await context.Recipes.AnyAsync(r => r.Id == message.Request.RecipeId, cancellationToken);
            if (!recipeExists)
            {
                return Result.Fail(new NotFoundError());
            }

            schedule.RecipeId = message.Request.RecipeId;
            schedule.Name = message.Request.Name;
            schedule.Type = message.Request.Type;
            schedule.Interval = message.Request.Type == JobScheduleTypeEnum.Repeat ? message.Request.Interval : null;
            schedule.IntervalValue = message.Request.Type == JobScheduleTypeEnum.Repeat ? message.Request.IntervalValue : null;
            schedule.StartTime = message.Request.StartTime;
            schedule.EndTime = message.Request.EndTime;
            schedule.Cycles = message.Request.Cycles;
            schedule.IsActive = message.Request.IsActive;

            if (schedule.WeekDays.Count > 0)
            {
                context.JobScheduleWeekDays.RemoveRange(schedule.WeekDays);
                schedule.WeekDays.Clear();
            }

            if (message.Request.Type == JobScheduleTypeEnum.Repeat && message.Request.Interval == JobScheduleIntervalEnum.Week)
            {
                var days = message.Request.DaysOfWeek ?? new List<JobScheduleWeekDayEnum>();
                schedule.WeekDays = days.Select(day => new JobScheduleWeekDay { JobScheduleId = schedule.Id, Day = day }).ToList();
            }
            else
            {
                schedule.WeekDays = [];
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(schedule.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ScheduleId).NotEmpty();
            RuleFor(x => x.Request.DeviceId).NotEmpty();
            RuleFor(x => x.Request.RecipeId).NotEmpty();
            RuleFor(x => x.Request.StartTime).NotEmpty();
            RuleFor(x => x.Request.Cycles).GreaterThan(0);
            RuleFor(x => x.Request.EndTime)
                .Must((command, endTime) => !endTime.HasValue || endTime > command.Request.StartTime)
                .WithMessage("End time must be after start time");

            When(
                x => x.Request.Type == JobScheduleTypeEnum.Repeat,
                () =>
                {
                    RuleFor(x => x.Request.Interval).NotNull().WithMessage("Interval is required for repeat schedules");

                    RuleFor(x => x.Request.IntervalValue)
                        .NotNull()
                        .WithMessage("Interval value is required for repeat schedules")
                        .GreaterThan(0)
                        .WithMessage("Interval value must be greater than 0");

                    When(
                        x => x.Request.Interval == JobScheduleIntervalEnum.Week,
                        () =>
                        {
                            RuleFor(x => x.Request.DaysOfWeek)
                                .NotNull()
                                .WithMessage("Days of week are required for weekly schedules")
                                .Must(days => days != null && days.Count > 0)
                                .WithMessage("At least one day must be provided for weekly schedules")
                                .Must(days => days != null && days.Distinct().Count() == days.Count)
                                .WithMessage("Duplicate days are not allowed");
                        }
                    );
                }
            );
        }
    }
}
