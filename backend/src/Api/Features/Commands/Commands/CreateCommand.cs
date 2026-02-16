using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Commands.Commands;

public static class CreateCommand
{
    public record Request(string DisplayName, string Name, Guid DeviceTemplateId, List<double> Params);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "commands",
                    async Task<Results<Created<Guid>, ValidationProblem, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request
                    ) =>
                    {
                        var command = new Command(request, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateCommand))
                .WithTags(nameof(Command))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a command";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var deviceTemplate = await context
                .DeviceTemplates.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == message.Request.DeviceTemplateId, cancellationToken);

            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!deviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var createdCommand = new Data.Models.Command
            {
                DisplayName = message.Request.DisplayName,
                Name = message.Request.Name,
                DeviceTemplateId = message.Request.DeviceTemplateId,
                Params = message.Request.Params,
            };

            await context.Commands.AddAsync(createdCommand, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(createdCommand.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(x => x.Request.DeviceTemplateId).NotEmpty().WithMessage("Device template ID is required");
        }
    }
}
