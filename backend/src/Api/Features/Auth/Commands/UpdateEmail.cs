using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Features.Auth.Commands;

public static class UpdateEmail
{
    public record Request(string NewEmail);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "auth/email",
                    async Task<Results<Ok, NotFound, ValidationProblem, ForbidHttpResult>> (
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
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok();
                    }
                )
                .WithName(nameof(UpdateEmail))
                .WithTags(nameof(Auth))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update user e-mail";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result>;

    public class Handler(UserManager<ApplicationUser> userManager, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var user = await userManager.FindByIdAsync(message.User.GetUserId().ToString());
            if (user == null)
            {
                return Result.Fail(new NotFoundError());
            }

            user.Email = message.Request.NewEmail;
            var identityResult = await userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                return Result.Fail(new ValidationError(identityResult));
            }


            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.NewEmail).NotEmpty().EmailAddress();
        }
    }
}
