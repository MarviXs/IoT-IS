using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Commands;

public static class UpdateDeviceTemplate
{
    public record Request(
        string Name,
        DeviceType DeviceType = DeviceType.Generic,
        bool EnableMap = false,
        bool EnableGrid = false,
        int? GridRowSpan = null,
        int? GridColumnSpan = null
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "device-templates/{id:guid}",
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
                .WithName(nameof(UpdateDeviceTemplate))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a device";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid TemplateId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var deviceTemplate = await context.DeviceTemplates.FindAsync([message.TemplateId], cancellationToken);
            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!deviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            deviceTemplate.Name = message.Request.Name;
            deviceTemplate.DeviceType = message.Request.DeviceType;
            deviceTemplate.EnableMap = message.Request.EnableMap;
            deviceTemplate.EnableGrid = message.Request.EnableGrid;
            deviceTemplate.GridRowSpan = message.Request.EnableGrid ? message.Request.GridRowSpan : null;
            deviceTemplate.GridColumnSpan = message.Request.EnableGrid ? message.Request.GridColumnSpan : null;

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Fail(new ConcurrencyError());
            }

            return Result.Ok();
        }
    }

    public record UpdateDeviceTemplateResponse(Guid Id, string Name);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Request.GridRowSpan)
                .NotNull()
                .When(x => x.Request.EnableGrid)
                .WithMessage("Grid row span is required when grid is enabled");
            RuleFor(x => x.Request.GridColumnSpan)
                .NotNull()
                .When(x => x.Request.EnableGrid)
                .WithMessage("Grid column span is required when grid is enabled");
            RuleFor(x => x.Request.GridRowSpan)
                .GreaterThan(0)
                .When(x => x.Request.GridRowSpan.HasValue)
                .WithMessage("Grid row span must be greater than 0");
            RuleFor(x => x.Request.GridColumnSpan)
                .GreaterThan(0)
                .When(x => x.Request.GridColumnSpan.HasValue)
                .WithMessage("Grid column span must be greater than 0");
        }
    }
}
