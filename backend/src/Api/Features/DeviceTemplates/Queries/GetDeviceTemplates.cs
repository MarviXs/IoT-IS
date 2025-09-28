using System.Linq.Dynamic.Core;
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
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Queries;

public static class GetDeviceTemplates
{
    public class QueryParameters : SearchParameters { }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "device-templates",
                    async Task<Results<Ok<PagedList<Response>>, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetDeviceTemplates))
                .WithTags(nameof(DeviceTemplates))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all device templates";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters TemplateParameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, IValidator<QueryParameters> validator) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var templateParameters = message.TemplateParameters;

            var result = validator.Validate(templateParameters);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var query = context
                .DeviceTemplates.AsNoTracking()
                .Where(d => d.OwnerId == message.User.GetUserId())
                .Where(d => d.Name.ToLower().Contains(StringUtils.Normalized(templateParameters.SearchTerm)))
                .Sort(templateParameters.SortBy ?? nameof(DeviceTemplate.UpdatedAt), templateParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var templates = await query
                .Paginate(templateParameters)
                .Select(
                    template => new Response(
                        template.Id,
                        template.Name,
                        template.UpdatedAt,
                        template.GridRowSpan,
                        template.GridColumnSpan
                    )
                )
                .ToListAsync(cancellationToken);

            return Result.Ok(templates.ToPagedList(totalCount, templateParameters.PageNumber, templateParameters.PageSize));
        }
    }

    public record Response(Guid Id, string Name, DateTime UpdatedAt, int? GridRowSpan, int? GridColumnSpan);

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields =
        [
            nameof(DeviceTemplate.Name),
            nameof(DeviceTemplate.CreatedAt),
            nameof(DeviceTemplate.UpdatedAt)
        ];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
