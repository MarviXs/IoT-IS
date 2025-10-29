using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceTemplates.Queries;

public static class GetAdminDeviceTemplates
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "admin/device-templates",
                    async Task<Ok<PagedList<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(GetAdminDeviceTemplates))
                .WithTags(nameof(DeviceTemplate))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all device templates for administrators";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<PagedList<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var templateParameters = message.Parameters;
            var normalizedSearch = StringUtils.Normalized(templateParameters.SearchTerm);

            var query = context
                .DeviceTemplates.AsNoTracking()
                .Include(template => template.Owner)
                .Where(
                    template =>
                        template.Name.ToLower().Contains(normalizedSearch)
                        || (template.Owner != null
                            && template.Owner.Email != null
                            && template.Owner.Email.ToLower().Contains(normalizedSearch))
                )
                .Sort(templateParameters.SortBy ?? nameof(DeviceTemplate.UpdatedAt), templateParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var templates = await query.Paginate(templateParameters).ToListAsync(cancellationToken);

            var responseTemplates = templates
                .Select(
                    template =>
                        new Response(
                            template.Id,
                            template.Name,
                            template.OwnerId,
                            template.Owner?.Email,
                            template.UpdatedAt,
                            template.EnableMap,
                            template.EnableGrid,
                            template.GridRowSpan,
                            template.GridColumnSpan,
                            template.IsGlobal
                        )
                )
                .ToList();

            return responseTemplates.ToPagedList(totalCount, templateParameters.PageNumber, templateParameters.PageSize);
        }
    }

    public record Response(
        Guid Id,
        string Name,
        Guid OwnerId,
        string? OwnerEmail,
        DateTime UpdatedAt,
        bool EnableMap,
        bool EnableGrid,
        int? GridRowSpan,
        int? GridColumnSpan,
        bool IsGlobal
    );
}
