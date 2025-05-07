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

namespace Fei.Is.Api.Features.EditorPlants.Commands;

public static class CreateEditorPlant
{
    public record Request(
        string PlantID,
        string Name,
        string Type,
        int Width,
        int Height,
        int PosX,
        int PosY,
        DateTime DateCreated,
        int CurrentDay,
        string Stage,
        string CurrentState,
        string EditorBoardId,
        Guid GreenHouseId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "editorplants",
                    async Task<Results<Created<Guid>, ValidationProblem, Conflict<string>>> (
                        IMediator mediator, ClaimsPrincipal user, Request request) =>
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
                .WithName(nameof(CreateEditorPlant))
                .WithTags(nameof(EditorPlant))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create an editor plant";
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
            var validation = await validator.ValidateAsync(message, cancellationToken);
            if (!validation.IsValid)
            {
                return Result.Fail(new ValidationError(validation));
            }

            var existing = await context.EditorPlants
                .FirstOrDefaultAsync(p => p.PlantID == message.Request.PlantID, cancellationToken);

            if (existing != null)
            {
                return Result.Ok(existing.Id);
            }

            var plant = new EditorPlant
            {
                PlantID = message.Request.PlantID,
                Name = message.Request.Name,
                Type = message.Request.Type,
                Width = message.Request.Width,
                Height = message.Request.Height,
                PosX = message.Request.PosX,
                PosY = message.Request.PosY,
                DateCreated = message.Request.DateCreated,
                CurrentDay = message.Request.CurrentDay,
                Stage = message.Request.Stage,
                CurrentState = message.Request.CurrentState,
                EditorBoardId = message.Request.EditorBoardId,
                GreenHouseId = message.Request.GreenHouseId
            };

            await context.EditorPlants.AddAsync(plant, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(plant.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.PlantID).NotEmpty();
            RuleFor(r => r.Request.Name).NotEmpty();
            RuleFor(r => r.Request.Type).NotEmpty();
            RuleFor(r => r.Request.Width).GreaterThan(0);
            RuleFor(r => r.Request.Height).GreaterThan(0);
            RuleFor(r => r.Request.Stage).NotEmpty();
            RuleFor(r => r.Request.CurrentState).NotEmpty();
            RuleFor(r => r.Request.EditorBoardId).MaximumLength(100);
            RuleFor(r => r.Request.GreenHouseId).NotEmpty();
        }
    }
}
