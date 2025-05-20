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

namespace Fei.Is.Api.Features.GreenHouses.Commands;

public static class CreateGreenHouse
{
    public record Request(Guid GreenHouseID, string Name, int Width, int Depth);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "greenhouses",
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
                .WithName(nameof(CreateGreenHouse))
                .WithTags(nameof(GreenHouse))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a greenhouse";
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

            var existing = await context.Greenhouses.FirstOrDefaultAsync(g => g.GreenHouseID == message.Request.GreenHouseID, cancellationToken);

            if (existing != null)
            {
                return Result.Ok(existing.Id);
            }

            var greenhouse = new GreenHouse
            {
                GreenHouseID = message.Request.GreenHouseID,
                Name = message.Request.Name,
                Width = message.Request.Width,
                Depth = message.Request.Depth,
                DateCreated = DateTime.UtcNow
            };

            await context.Greenhouses.AddAsync(greenhouse, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(greenhouse.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.Name).NotEmpty().WithMessage("Name is required");
            RuleFor(r => r.Request.GreenHouseID).NotEmpty().WithMessage("GreenHouseId is required");
            RuleFor(r => r.Request.Width).GreaterThan(0).WithMessage("Width must be greater than 0");
            RuleFor(r => r.Request.Depth).GreaterThan(0).WithMessage("Depth must be greater than 0");
        }
    }
}
