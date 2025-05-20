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

public static class CreatePlant
{
    public record Request(Guid PlantId, string Name, string Type, DateTime DatePlanted, Guid PlantBoardId);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "lifecycles/plants",
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
                .WithName(nameof(CreatePlant))
                .WithTags(nameof(Plant))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a plant";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var existingPlant = await context.Plants.FirstOrDefaultAsync(p => p.PlantId == message.Request.PlantId, cancellationToken);

            if (existingPlant != null)
            {
                return Result.Ok(existingPlant.Id);
            }

            var plantBoard = await context.PlantBoards.FirstOrDefaultAsync(pb => pb.PlantBoardId == message.Request.PlantBoardId, cancellationToken);

            if (plantBoard == null)
            {
                return Result.Fail(new BadRequestError("Invalid PlantBoardId"));
            }

            var plant = new Plant
            {
                PlantId = message.Request.PlantId,
                Name = message.Request.Name,
                Type = message.Request.Type,
                DatePlanted = message.Request.DatePlanted,
                PlantBoardId = message.Request.PlantBoardId,
                PlantBoard = plantBoard
            };

            await context.Plants.AddAsync(plant, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(plant.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(r => r.Request.Type).NotEmpty().WithMessage("Type is required");
            RuleFor(r => r.Request.DatePlanted).NotEmpty().WithMessage("DatePlanted is required");
            RuleFor(r => r.Request.PlantBoardId).NotEmpty().WithMessage("PlantBoardId is required");
        }
    }
}
