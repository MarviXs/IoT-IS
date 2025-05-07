using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Queries
{
    /// <summary>
    /// Returns only the EAN code for a product.
    /// </summary>
    public static class GetProductEan
    {
        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                        "products/ean/{id:Guid}",
                        async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, Guid id) =>
                        {
                            var query = new Query(id);
                            var result = await mediator.Send(query);
                            if (result.HasError<NotFoundError>())
                                return TypedResults.NotFound();

                            return TypedResults.Ok(result.Value);
                        }
                    )
                    .WithName(nameof(GetProductEan))
                    .WithTags(nameof(Product))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Get a product's EAN code by id";
                        return o;
                    });
            }
        }

        public record Query(Guid Id) : IRequest<Result<Response>>;

        public sealed class Handler : IRequestHandler<Query, Result<Response>>
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Fetch only EANCode from database
                var eanCode = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Id == request.Id)
                    .Select(p => p.EANCode)
                    .FirstOrDefaultAsync(cancellationToken);

                if (eanCode == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                var response = new Response(
                    EANCode: eanCode
                );

                return Result.Ok(response);
            }
        }

        public record Response(
            string EANCode
        );
    }
}
