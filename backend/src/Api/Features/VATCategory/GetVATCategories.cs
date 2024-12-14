using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.VATCategory.Queries;

public static class GetVATCategories
{
    public class QueryParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "vat-category",
                    async Task<Ok<List<Response>>> (IMediator mediator, ClaimsPrincipal user, [AsParameters] QueryParameters parameters) =>
                    {
                        var query = new Query(user, parameters);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetVATCategories))
                .WithTags(nameof(VATCategory))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get VAT categories";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<List<Response>>;

    public sealed class Handler(AppDbContext context, IConfiguration configuration) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context.VATCategories.AsNoTracking().Select(vatCategory => new Response(vatCategory.Id, vatCategory.Name, vatCategory.Rate));
            return await query.ToListAsync(cancellationToken);
        }
    }

    public record Response(Guid Id, string Name, decimal Rate);
}
