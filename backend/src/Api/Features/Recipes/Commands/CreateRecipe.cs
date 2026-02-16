using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Features.DeviceTemplates.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.Recipes.Commands;

public static class CreateRecipe
{
    public record Request(string Name, Guid DeviceTemplateId);

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "recipes",
                    async Task<Results<Created<Guid>, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request
                    ) =>
                    {
                        var command = new Command(request, user);

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

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateRecipe))
                .WithTags(nameof(Recipe))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a recipe";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var deviceTemplate = await context.DeviceTemplates.FindAsync([message.Request.DeviceTemplateId], cancellationToken: cancellationToken);
            if (deviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }
            if (!deviceTemplate.CanEdit(message.User))
            {
                return Result.Fail(new ForbiddenError());
            }

            var recipe = new Recipe { Name = message.Request.Name, DeviceTemplateId = message.Request.DeviceTemplateId };

            await context.Recipes.AddAsync(recipe, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(recipe.Id);
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
