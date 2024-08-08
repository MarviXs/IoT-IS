using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.UserManagement.Queries;

public static class GetUsers
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "admin/users",
                    async Task<Results<Ok<PagedList<Response>>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, parameters);
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
                .WithName(nameof(GetUsers))
                .WithTags(nameof(ApplicationUser))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all registered users";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, UserManager<ApplicationUser> userManager, IValidator<QueryParameters> validator)
        : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var queryParameters = message.Parameters;

            var result = validator.Validate(queryParameters);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var usersQuery = userManager
                .Users.AsNoTracking()
                .Include(d => d.UserRoles!)
                .ThenInclude(d => d.Role)
                .Where(d => d.Email!.ToLower().Contains(StringUtils.Normalized(queryParameters.SearchTerm)));

            var totalCount = await usersQuery.CountAsync(cancellationToken);

            var users = await usersQuery
                .Sort(queryParameters.SortBy ?? nameof(ApplicationUser.Email), queryParameters.Descending)
                .Paginate(queryParameters)
                .Select(d => new Response(d.Id, d.Email, d.RegistrationDate, d.UserRoles!.Select(e => e.Role.Name)))
                .ToListAsync(cancellationToken);

            return Result.Ok(users.ToPagedList(totalCount, queryParameters.PageNumber, queryParameters.PageSize));
        }
    }

    public record Response(Guid Id, string Email, DateTimeOffset RegistrationDate, IEnumerable<string> Roles);

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields = [nameof(ApplicationUser.Email), nameof(ApplicationUser.RegistrationDate)];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
