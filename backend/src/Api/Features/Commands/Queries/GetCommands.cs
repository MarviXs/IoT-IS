using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Commands.Queries;

public static class GetCommands
{
    public class QueryParameters : SearchParameters
    {
        public Guid? DeviceTemplateId { get; init; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "commands",
                    async Task<Results<Ok<PagedList<Response>>, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters paramaters
                    ) =>
                    {
                        var query = new Query(user, paramaters);
                        var result = await mediator.Send(query);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetCommands))
                .WithTags(nameof(Command))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all commands";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, IValidator<QueryParameters> validator) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var queryParameters = message.Parameters;

            var result = validator.Validate(queryParameters);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var query = context
                .Commands.AsNoTracking()
                .Include(d => d.DeviceTemplate)
                .AsQueryable();

            if (!message.User.IsAdmin())
            {
                var userId = message.User.GetUserId();
                query = query.Where(d => d.DeviceTemplate!.OwnerId == userId);
            }

            query = query
                .Where(d => d.Name.ToLower().Contains(StringUtils.Normalized(queryParameters.SearchTerm)))
                .Where(d => queryParameters.DeviceTemplateId == null || d.DeviceTemplateId == queryParameters.DeviceTemplateId)
                .Sort(queryParameters.SortBy ?? nameof(Command.UpdatedAt), queryParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var commands = await query
                .Paginate(queryParameters)
                .Select(command => new Response(command.Id, command.DisplayName, command.Name, command.Params))
                .ToListAsync(cancellationToken);

            return Result.Ok(commands.ToPagedList(totalCount, queryParameters.PageNumber, queryParameters.PageSize));
        }
    }

    public record Response(Guid Id, string DisplayName, string Name, List<double> Params);

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields =
        [
            nameof(Command.DisplayName),
            nameof(Command.Name),
            nameof(Command.CreatedAt),
            nameof(Command.UpdatedAt)
        ];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
