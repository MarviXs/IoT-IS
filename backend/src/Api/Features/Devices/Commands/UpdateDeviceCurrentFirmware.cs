using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Commands;

public static class UpdateDeviceCurrentFirmware
{
    public record Request(string AccessToken, string VersionNumber);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/firmwares/current",
                    async Task<Results<Ok, ValidationProblem, NotFound>> (IMediator mediator, Request request) =>
                    {
                        var command = new Command(request);
                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok();
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(UpdateDeviceCurrentFirmware))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update the current firmware version reported by a device";
                    return o;
                });
        }
    }

    public record Command(Request Request) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var device = await context.Devices.FirstOrDefaultAsync(
                d => d.AccessToken == message.Request.AccessToken,
                cancellationToken
            );

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            device.CurrentFirmwareVersion = message.Request.VersionNumber;
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Request.AccessToken).NotEmpty().WithMessage("Access token is required");
            RuleFor(c => c.Request.VersionNumber).NotEmpty().WithMessage("Version number is required");
        }
    }
}
