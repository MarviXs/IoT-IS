using System.Linq;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.JobSchedules.Queries;

public static class GetJobScheduleById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "job-schedules/{scheduleId:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid scheduleId) =>
                    {
                        var query = new Query(scheduleId, user);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetJobScheduleById))
                .WithTags(nameof(JobSchedule))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a job schedule";
                    return o;
                });
        }
    }

    public record Query(Guid ScheduleId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var schedule = await context
                .JobSchedules.AsNoTracking()
                .Include(s => s.Device).ThenInclude(d => d!.SharedWithUsers)
                .Include(s => s.WeekDays)
                .FirstOrDefaultAsync(s => s.Id == message.ScheduleId, cancellationToken);
            if (schedule == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var device = schedule.Device;
            if (device == null || !device.CanView(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var response = new Response(
                schedule.Id,
                schedule.DeviceId,
                schedule.RecipeId,
                schedule.Type,
                schedule.Interval,
                schedule.IntervalValue,
                schedule.StartTime,
                schedule.EndTime,
                schedule.WeekDays.OrderBy(day => day.Day).Select(day => day.Day).ToList(),
                schedule.Cycles,
                schedule.IsActive
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        Guid DeviceId,
        Guid RecipeId,
        JobScheduleTypeEnum Type,
        JobScheduleIntervalEnum? Interval,
        int? IntervalValue,
        DateTimeOffset StartTime,
        DateTimeOffset? EndTime,
        List<JobScheduleWeekDayEnum> DaysOfWeek,
        int Cycles,
        bool IsActive
    );
}
