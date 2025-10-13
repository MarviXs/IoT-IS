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

public static class GetJobSchedules
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "devices/{deviceId:guid}/job-schedules",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid deviceId) =>
                    {
                        var query = new Query(deviceId, user);
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
                .WithName(nameof(GetJobSchedules))
                .WithTags(nameof(JobSchedule))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get job schedules for a device";
                    return o;
                });
        }
    }

    public record Query(Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var device = await context.Devices.Include(d => d.SharedWithUsers).FirstOrDefaultAsync(d => d.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (!device.CanView(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var schedules = await context
                .JobSchedules.AsNoTracking()
                .Where(schedule => schedule.DeviceId == message.DeviceId)
                .Include(schedule => schedule.WeekDays)
                .OrderBy(schedule => schedule.StartTime)
                .ToListAsync(cancellationToken);

            var response = schedules
                .Select(schedule => new Response(
                    schedule.Id,
                    schedule.Name,
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
                ))
                .ToList();

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        string Name,
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
