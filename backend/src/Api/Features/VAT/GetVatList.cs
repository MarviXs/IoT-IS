using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Queries;

public static class GetVatList
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "vat/",
                    async Task<Results<Ok<List<Response>>, NotFound, ForbidHttpResult>> (IMediator mediator) =>
                    {
                        var query = new Query();
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetVatList))
                .WithTags(nameof(VATCategory))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get VAT categories";
                    return o;
                });
        }
    }

    public record Query() : IRequest<Result<List<Response>>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var response = context.VATCategories.Select(vat => new Response(vat.Id, vat.Name, vat.Rate));

            return Result.Ok(await response.ToListAsync(cancellationToken));
        }
    }

    public record Response(Guid Guid, string Name, decimal Rate);
}
