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

public static class UpdateDeviceTemplate
{
    public record Request(string Name, string ModelId);

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
            if (deviceTemplate.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            deviceTemplate.Name = message.Request.Name;
            deviceTemplate.ModelId = message.Request.ModelId;

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

    public record UpdateDeviceTemplateResponse(Guid Id, string Name, string ModelId);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty();
            RuleFor(x => x.Request.ModelId).NotEmpty();
        }
    }
}
