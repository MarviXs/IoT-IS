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

namespace Fei.Is.Api.Features.Devices.Commands;

public static class UpdateDevice
{
    public record Request(string Name, string AccessToken, Guid? TemplateId, DeviceConnectionProtocol Protocol, int? DataPointRetentionDays);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "devices/{id:guid}",
                    async Task<Results<NoContent, NotFound, ValidationProblem, Conflict>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid id,
                        Request request
                    ) =>
                    {
                        var command = new Command(request, id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        else if (result.HasError<ConcurrencyError>())
                        {
                            return TypedResults.Conflict();
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateDevice))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a device";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid DeviceId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var device = await context.Devices.Include(d => d.SharedWithUsers).FirstOrDefaultAsync(d => d.Id == message.DeviceId, cancellationToken);
            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!device.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var template = await context.DeviceTemplates.FindAsync([message.Request.TemplateId], cancellationToken);
            if (message.Request.TemplateId.HasValue && template == null)
            {
                return Result.Fail(new NotFoundError());
            }

            device.Name = message.Request.Name;
            device.AccessToken = message.Request.AccessToken;
            device.DeviceTemplateId = message.Request.TemplateId;
            device.Protocol = message.Request.Protocol;
            device.DataPointRetentionDays = message.Request.DataPointRetentionDays;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public record Response(Guid Id, string Name, long? ResponseTime, long? LastResponseTimestamp);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.DeviceId).NotEmpty().WithMessage("Device ID is required");
            RuleFor(r => r.Request.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(r => r.Request.AccessToken).NotEmpty().WithMessage("Access token is required");
            RuleFor(r => r.Request.DataPointRetentionDays)
                .GreaterThan(0)
                .WithMessage("Data point retention must be greater than zero days")
                .When(r => r.Request.DataPointRetentionDays.HasValue);
        }
    }
}
