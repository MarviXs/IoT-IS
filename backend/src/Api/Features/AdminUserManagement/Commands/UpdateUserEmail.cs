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

public static class UpdateUserEmail
{
    public record Request(string Email);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "admin/users/{id:guid}/email",
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
                .WithName(nameof(UpdateUserEmail))
                .WithTags(nameof(ApplicationUser))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update user email";
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

            user.Email = message.Request.Email;

            var resultUpdate = await userManager.UpdateAsync(user);

            if (!resultUpdate.Succeeded)
            {
                return Result.Fail(new ValidationError(resultUpdate));
            }

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.Email).EmailAddress();
        }
    }
}
