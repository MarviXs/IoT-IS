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

public static class CreateEditorPlants
{
    public record Request(
        Guid PlantID,
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
        Guid EditorBoardId,
        Guid GreenHouseId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "editorplants",
                    async Task<Results<Created<List<Guid>>, ValidationProblem, Conflict<string>>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        List<Request> requests
                    ) =>
                    {
                        var command = new Command(requests, user);

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

                        return TypedResults.Created("/editorplants", result.Value);
                    }
                )
                .WithName(nameof(CreateEditorPlants))
                .WithTags(nameof(EditorPlant))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create multiple editor plants";
                    return o;
                });
        }
    }

    public record Command(List<Request> Requests, ClaimsPrincipal User) : IRequest<Result<List<Guid>>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<List<Guid>>>
    {
        public async Task<Result<List<Guid>>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validation = await validator.ValidateAsync(message, cancellationToken);
            if (!validation.IsValid)
            {
                return Result.Fail(new ValidationError(validation));
            }

            var createdIds = new List<Guid>();

            foreach (var request in message.Requests)
            {
                var existing = await context.EditorPlants.FirstOrDefaultAsync(p => p.PlantID == request.PlantID, cancellationToken);

                if (existing != null)
                {
                    createdIds.Add(existing.Id);
                    continue;
                }

                var plant = new EditorPlant
                {
                    PlantID = request.PlantID,
                    Name = request.Name,
                    Type = request.Type,
                    Width = request.Width,
                    Height = request.Height,
                    PosX = request.PosX,
                    PosY = request.PosY,
                    DateCreated = request.DateCreated,
                    CurrentDay = request.CurrentDay,
                    Stage = request.Stage,
                    CurrentState = request.CurrentState,
                    EditorBoardId = request.EditorBoardId,
                    GreenHouseId = request.GreenHouseId
                };

                await context.EditorPlants.AddAsync(plant, cancellationToken);
                createdIds.Add(plant.Id);
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(createdIds);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleForEach(r => r.Requests).SetValidator(new RequestValidator());
        }
    }

    public sealed class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.PlantID).NotEmpty();
            RuleFor(r => r.Name).NotEmpty();
            RuleFor(r => r.Type).NotEmpty();
            RuleFor(r => r.Width).GreaterThan(0);
            RuleFor(r => r.Height).GreaterThan(0);
            RuleFor(r => r.Stage).NotEmpty();
            RuleFor(r => r.CurrentState).NotEmpty();
            RuleFor(r => r.GreenHouseId).NotEmpty();
        }
    }
}
