using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Features.UserManagement.Queries;

public static class GetUserById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "admin/users/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(user, id);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(GetUserById))
                .WithTags(nameof(ApplicationUser))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get user by ID";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid UserId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, UserManager<ApplicationUser> userManager) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(message.UserId.ToString());
            if (user is null)
            {
                return Result.Fail(new NotFoundError());
            }

            var roles = await userManager.GetRolesAsync(user);
            var response = new Response(user.Id, user.Email, user.RegistrationDate, roles);
            return Result.Ok(response);
        }
    }

    public record Response(Guid Id, string Email, DateTimeOffset RegistrationDate, IEnumerable<string> Roles);
}
