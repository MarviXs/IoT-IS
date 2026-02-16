using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Commands;

public static class UpdateEdgeNode
{
    public record Request(string Name, string Token);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "system/edge-nodes/{id:guid}",
                    async Task<Results<NoContent, NotFound, ValidationProblem>> (
                        IMediator mediator,
                        Guid id,
                        Request request,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var result = await mediator.Send(new Command(id, request), cancellationToken);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(UpdateEdgeNode))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Update edge node setting";
                    o.Description = "Updates edge node name and token.";
                    return o;
                });
        }
    }

    public record Command(Guid EdgeNodeId, Request Request) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var edgeNode = await context.EdgeNodes.FirstOrDefaultAsync(node => node.Id == message.EdgeNodeId, cancellationToken);
            if (edgeNode == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var token = message.Request.Token.Trim();
            var tokenExists = await context.EdgeNodes.AnyAsync(node => node.Token == token && node.Id != edgeNode.Id, cancellationToken);
            if (tokenExists)
            {
                return Result.Fail(new ValidationError(nameof(Request.Token), "Token is already in use"));
            }

            edgeNode.Name = message.Request.Name.Trim();
            edgeNode.Token = token;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.EdgeNodeId).NotEmpty();
            RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(128);
            RuleFor(command => command.Request.Token).NotEmpty().MaximumLength(256);
        }
    }
}
