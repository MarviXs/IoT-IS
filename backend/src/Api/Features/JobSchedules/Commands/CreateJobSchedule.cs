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

public static class CreateJobSchedule
{
    public record Request(
        Guid RecipeId,
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
            app.MapPost(
                    "devices/{deviceId:guid}/job-schedules",
                    async Task<Results<Created<Guid>, NotFound, ForbidHttpResult, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid deviceId
                    ) =>
                    {
                        var command = new Command(deviceId, user, request);
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
                .WithName(nameof(CreateJobSchedule))
                .WithTags(nameof(JobSchedule))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a job schedule";
                    return o;
                });
        }
    }

    public record Command(Guid DeviceId, ClaimsPrincipal User, Request Request) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var device = await context
                .Devices.Include(d => d.SharedWithUsers)
                .FirstOrDefaultAsync(device => device.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!device.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var recipeExists = await context.Recipes.AnyAsync(r => r.Id == message.Request.RecipeId, cancellationToken);
            if (!recipeExists)
            {
                return Result.Fail(new NotFoundError());
            }

            var schedule = new JobSchedule
            {
                DeviceId = message.DeviceId,
                RecipeId = message.Request.RecipeId,
                Type = message.Request.Type,
                Interval = message.Request.Type == JobScheduleTypeEnum.Repeat ? message.Request.Interval : null,
                IntervalValue = message.Request.Type == JobScheduleTypeEnum.Repeat ? message.Request.IntervalValue : null,
                StartTime = message.Request.StartTime,
                EndTime = message.Request.EndTime,
                Cycles = message.Request.Cycles,
                IsActive = message.Request.IsActive,
                WeekDays = []
            };

            if (message.Request.Type == JobScheduleTypeEnum.Repeat && message.Request.Interval == JobScheduleIntervalEnum.Week)
            {
                var days = message.Request.DaysOfWeek ?? new List<JobScheduleWeekDayEnum>();
                schedule.WeekDays = days.Select(day => new JobScheduleWeekDay { Day = day }).ToList();
            }

            await context.JobSchedules.AddAsync(schedule, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(schedule.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.DeviceId).NotEmpty();
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
