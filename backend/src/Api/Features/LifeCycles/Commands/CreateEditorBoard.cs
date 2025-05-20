using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.EditorBoards.Commands;

public static class CreateEditorBoard
{
    public record Request(
        Guid EditorBoardID,
        string Name,
        int Columns,
        int Rows,
        int Width,
        int Height,
        int PosX,
        int PosY,
        string Shape,
        DateTime DateCreated,
        Guid GreenHouseId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "editorboards",
                    async Task<Results<Created<Guid>, ValidationProblem, Conflict<string>>> (
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

                        if (result.HasError<BadRequestError>())
                        {
                            var errorMessage = result.Errors.FirstOrDefault()?.Message;
                            return TypedResults.Conflict(errorMessage);
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateEditorBoard))
                .WithTags(nameof(EditorBoard))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create an editor board";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validation = await validator.ValidateAsync(message, cancellationToken);
            if (!validation.IsValid)
            {
                return Result.Fail(new ValidationError(validation));
            }

            var existing = await context.EditorPots.FirstOrDefaultAsync(b => b.EditorBoardID == message.Request.EditorBoardID, cancellationToken);

            if (existing != null)
            {
                return Result.Ok(existing.Id);
            }

            var board = new EditorBoard
            {
                EditorBoardID = message.Request.EditorBoardID,
                Name = message.Request.Name,
                Columns = message.Request.Columns,
                Rows = message.Request.Rows,
                Width = message.Request.Width,
                Height = message.Request.Height,
                PosX = message.Request.PosX,
                PosY = message.Request.PosY,
                Shape = message.Request.Shape,
                DateCreated = message.Request.DateCreated,
                GreenHouseId = message.Request.GreenHouseId
            };

            await context.EditorPots.AddAsync(board, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(board.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.Name).NotEmpty().MaximumLength(100);
            RuleFor(r => r.Request.Columns).GreaterThan(0);
            RuleFor(r => r.Request.Rows).GreaterThan(0);
            RuleFor(r => r.Request.Width).GreaterThan(0);
            RuleFor(r => r.Request.Height).GreaterThan(0);
            RuleFor(r => r.Request.PosX).GreaterThanOrEqualTo(0);
            RuleFor(r => r.Request.PosY).GreaterThanOrEqualTo(0);
            RuleFor(r => r.Request.Shape).NotEmpty().MaximumLength(100);
            RuleFor(r => r.Request.GreenHouseId).NotEmpty();
        }
    }
}
