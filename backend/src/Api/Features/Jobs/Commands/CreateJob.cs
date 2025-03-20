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

public static class CreateJob
{
    public record Request(Guid RecipeId, int Cycles);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/{deviceId:guid}/jobs",
                    async Task<Results<Created<Guid>, NotFound, ForbidHttpResult, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid deviceId
                    ) =>
                    {
                        var command = new Command(request, deviceId, user);
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
                .WithName(nameof(CreateJob))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a job";
                    o.Description = "Create a job for a device from a recipe.";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid DeviceId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, JobService jobService, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
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
                .FirstOrDefaultAsync(device => device.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var job = await jobService.CreateJobFromRecipe(message.DeviceId, message.Request.RecipeId, message.Request.Cycles, cancellationToken);
            if (job.IsFailed)
            {
                return Result.Fail(job.Errors);
            }
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(job.Value.Id);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.RecipeId).NotEmpty().WithMessage("Recipe ID is required");
            RuleFor(x => x.Request.Cycles).GreaterThan(0).WithMessage("Cycles must be greater than 0");
        }
    }
}
