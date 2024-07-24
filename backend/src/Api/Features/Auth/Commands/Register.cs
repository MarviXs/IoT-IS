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

public static class Register
{
    public sealed class Endpoint : ICarterModule
    {
        public record Request(string Email, string Password);

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "auth/register",
                    async Task<Results<Ok, Conflict, ValidationProblem>> (IMediator mediator, Request request) =>
                    {
                        var registerCommand = new Command(request.Email, request.Password);
                        var result = await mediator.Send(registerCommand);

                        if (result.HasError<UserAlreadyExistsError>())
                        {
                            return TypedResults.Conflict();
                        }
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok();
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(Register))
                .WithTags("Auth")
                .WithOpenApi(o =>
                {
                    o.Summary = "Register a new user";
                    o.Description = "Register a new user with an email and password";
                    return o;
                });
        }
    }

    public record Command(string Email, string Password) : IRequest<Result>;

    public class RegisterHandler(UserManager<User> userManager, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var user = new User { Email = message.Email, UserName = message.Email };

            var existingUser = await userManager.FindByEmailAsync(message.Email);
            if (existingUser != null)
            {
                return Result.Fail(new UserAlreadyExistsError());
            }

            var identityResult = await userManager.CreateAsync(user, message.Password);
            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.GroupBy(x => x.Code).ToDictionary(x => x.Key, x => x.Select(y => y.Description).ToArray());
                return Result.Fail(new ValidationError(errors));
            }

            // TODO: Remove in production
            if (identityResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
