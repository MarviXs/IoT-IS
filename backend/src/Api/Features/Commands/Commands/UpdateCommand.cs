using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.Commands.Commands;

public static class UpdateCommand
{
    public record Request(string DisplayName, string Name, List<double> Params);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "commands/{id:guid}",
                    async Task<Results<NoContent, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Guid id,
                        Request request
                    ) =>
                    {
                        var command = new Command(request, id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateCommand))
                .WithTags(nameof(Command))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a command";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid CommandId, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var command = await context.Commands.FindAsync([message.CommandId], cancellationToken);
            if (command == null)
            {
                return Result.Fail(new NotFoundError());
            }

            command.DisplayName = message.Request.DisplayName;
            command.Name = message.Request.Name;
            command.Params = message.Request.Params;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(command.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty();
        }
    }
}
