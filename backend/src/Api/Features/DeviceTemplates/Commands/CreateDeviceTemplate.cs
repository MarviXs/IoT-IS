using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Commands;

public static class CreateDeviceTemplate
{
    public record Request(
        string Name,
        DeviceType DeviceType = DeviceType.Generic,
        bool IsGlobal = false,
        bool EnableMap = false,
        bool EnableGrid = false,
        int? GridRowSpan = null,
        int? GridColumnSpan = null
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "device-templates",
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        var command = new Command(request, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateDeviceTemplate))
                .WithTags(nameof(DeviceTemplate))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a device template";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var template = new DeviceTemplate
            {
                OwnerId = message.User.GetUserId(),
                Name = message.Request.Name,
                DeviceType = message.Request.DeviceType,
                IsGlobal = message.User.IsAdmin() && message.Request.IsGlobal,
                EnableMap = message.Request.EnableMap,
                EnableGrid = message.Request.EnableGrid,
                GridRowSpan = message.Request.EnableGrid ? message.Request.GridRowSpan : null,
                GridColumnSpan = message.Request.EnableGrid ? message.Request.GridColumnSpan : null
            };

            await context.DeviceTemplates.AddAsync(template, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(template.Id);
        }
    }

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
