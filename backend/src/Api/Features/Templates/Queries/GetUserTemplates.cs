using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.Redis;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Templates.Queries;

public static class GetUserTemplates
{

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "templates",
                    async Task<Ok<List<Response>>> (IMediator mediator, ClaimsPrincipal user) =>
                    {
                        var query = new Query(user);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result);
                    }
                )
                .WithName(nameof(GetUserTemplates))
                .WithTags(nameof(UserFile))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get UserTemplates";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User) : IRequest<List<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, List<Response>>
    {
        public async Task<List<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var query = context
                .UserFiles.AsNoTracking()
                .Select(template => new Response(
                    template.FileIdentifier,
                    template.OriginalFileName // Prístup k názvu zákazníka cez navigačnú vlastnosť

                ));

            return query.ToList();
        }
    }

    public record Response(
        FileIdentifier Identifier,
        string FileName
    );
}
