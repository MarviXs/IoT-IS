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

namespace Fei.Is.Api.Features.AdminUserManagement.Commands;

public static class UpdateUserPassword
{
    public record Request(string Password);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "admin/users/{id:guid}/password",
                    async Task<Results<Ok, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        Request request,
                        Guid id
                    ) =>
                    {
                        var command = new Command(request, id);

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
                .RequireAuthorization("Admin")
                .WithName(nameof(UpdateUserPassword))
                .WithTags(nameof(ApplicationUser))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update user password";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid UserId) : IRequest<Result>;

    public class Handler(UserManager<ApplicationUser> userManager, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var user = await userManager.FindByIdAsync(message.UserId.ToString());
            if (user == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resultReset = await userManager.ResetPasswordAsync(user, token, message.Request.Password);

            if (!resultReset.Succeeded)
            {
                return Result.Fail(new ValidationError(resultReset));
            }

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.Password).NotEmpty();
        }
    }
}
