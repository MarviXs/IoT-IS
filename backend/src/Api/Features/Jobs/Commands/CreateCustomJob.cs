using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.Devices.Extensions;
using Fei.Is.Api.Features.Jobs.EventHandlers;
using Fei.Is.Api.Features.Jobs.Services;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Commands;

public static class CreateCustomJob
{
    public record Request(string JobName, long StartedAt, long FinishedAt);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/{deviceAccessToken}/jobs/custom",
                    async Task<Results<Created<Guid>, NotFound, ForbidHttpResult, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        string deviceAccessToken
                    ) =>
                    {
                        var command = new Command(request, deviceAccessToken, user);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateCustomJob))
                .WithTags(nameof(Job))
                .AllowAnonymous()
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a custom job";
                    o.Description = "Create a custom job from device without commands.";
                    return o;
                });
        }
    }

    public record Command(Request Request, string DeviceAccessToken, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var device = await context
                .Devices.Include(d => d.SharedWithUsers)
                .FirstOrDefaultAsync(device => device.AccessToken == message.DeviceAccessToken, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var job = new Job
            {
                DeviceId = device.Id,
                Name = message.Request.JobName,
                Status = JobStatusEnum.JOB_SUCCEEDED,
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTimeOffset.FromUnixTimeMilliseconds(message.Request.StartedAt).UtcDateTime,
                FinishedAt = DateTimeOffset.FromUnixTimeMilliseconds(message.Request.FinishedAt).UtcDateTime
            };
            context.Jobs.Add(job);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(job.Id);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.JobName).NotEmpty().WithMessage("Job name is required");
            RuleFor(x => x.Request.StartedAt).GreaterThan(0).WithMessage("StartedAt must be greater than 0");
            RuleFor(x => x.Request.FinishedAt).GreaterThan(0).WithMessage("FinishedAt must be greater than 0");
        }
    }
}
