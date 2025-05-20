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

namespace Fei.Is.Api.Features.LifeCycles.Commands;

public static class CreatePlantBoard
{
    public record Request(Guid PlantBoardId, int Rows, int Cols);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "lifecycles/plantboards",
                    async Task<Results<Created<Guid>, ValidationProblem, Conflict<string>>> (IMediator mediator, Request request) =>
                    {
                        var command = new Command(request);

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
                .WithName(nameof(CreatePlantBoard))
                .WithTags(nameof(PlantBoard))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a plant board";
                    return o;
                });
        }
    }

    public record Command(Request Request) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var existingBoard = await context.PlantBoards.FirstOrDefaultAsync(
                pb => pb.PlantBoardId == message.Request.PlantBoardId,
                cancellationToken
            );

            if (existingBoard != null)
            {
                return Result.Ok(existingBoard.Id);
            }

            var plantBoard = new PlantBoard
            {
                PlantBoardId = message.Request.PlantBoardId,
                Rows = message.Request.Rows,
                Cols = message.Request.Cols
            };

            await context.PlantBoards.AddAsync(plantBoard, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(plantBoard.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.PlantBoardId).NotEmpty().WithMessage("PlantBoardId is required");
            RuleFor(r => r.Request.Rows).GreaterThan(0).WithMessage("Rows must be greater than 0");
            RuleFor(r => r.Request.Cols).GreaterThan(0).WithMessage("Cols must be greater than 0");
        }
    }
}
