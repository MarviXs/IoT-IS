using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.Devices.Commands;

public static class CreateDevice
{
    public record Request(string Name, string AccessToken, Guid? TemplateId);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices",
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
                .WithName(nameof(CreateDevice))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a device";
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

            var templates = await context.DeviceTemplates.FindAsync(
                [message.Request.TemplateId, cancellationToken],
                cancellationToken: cancellationToken
            );
            if (message.Request.TemplateId.HasValue && templates == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var device = new Device
            {
                OwnerId = message.User.GetUserId(),
                Name = message.Request.Name,
                AccessToken = message.Request.AccessToken,
                DeviceTemplateId = message.Request.TemplateId
            };

            await context.Devices.AddAsync(device, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(device.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
