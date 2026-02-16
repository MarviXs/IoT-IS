using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Recipes.Commands;

public static class UpdateRecipe
{
    public record Request(string Name);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "recipes/{id:guid}",
                    async Task<Results<NoContent, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid id
                    ) =>
                    {
                        var command = new Command(request, id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateRecipe))
                .WithTags(nameof(Recipe))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update recipe";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid RecipeId, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var recipe = await context
                .Recipes.Include(r => r.DeviceTemplate)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == message.RecipeId, cancellationToken);
            if (recipe == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (recipe.DeviceTemplate == null || !recipe.DeviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            recipe.Name = message.Request.Name;

            await context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Name is required");
        }
    }
}
