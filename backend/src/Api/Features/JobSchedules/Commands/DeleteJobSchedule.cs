using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Features.Devices.Extensions;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.JobSchedules.Commands;

public static class DeleteJobSchedule
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete(
                    "job-schedules/{scheduleId:guid}",
                    async Task<Results<NoContent, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid scheduleId) =>
                    {
                        var command = new Command(scheduleId, user);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(DeleteJobSchedule))
                .WithTags(nameof(JobSchedule))
                .WithOpenApi(o =>
                {
                    o.Summary = "Delete a job schedule";
                    return o;
                });
        }
    }

    public record Command(Guid ScheduleId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var schedule = await context
                .JobSchedules.Include(s => s.Device).ThenInclude(d => d!.SharedWithUsers)
                .FirstOrDefaultAsync(s => s.Id == message.ScheduleId, cancellationToken);
            if (schedule == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var device = schedule.Device;
            if (device == null || !device.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            context.JobSchedules.Remove(schedule);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
