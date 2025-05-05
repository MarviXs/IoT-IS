using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Products.Queries
{
    /// <summary>
    /// Returns Latin name and location details for a product.
    /// </summary>
    public static class GetProductPassport
    {
        const string GardeningIdNumber = "GDN-12345";

        public sealed class Endpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet(
                        "products/passport/{id:Guid}",
                        async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (
                            IMediator mediator,
                            Guid id
                        ) =>
                        {
                            var query = new Query(id);
                            var result = await mediator.Send(query);
                            if (result.HasError<NotFoundError>())
                                return TypedResults.NotFound();

                            return TypedResults.Ok(result.Value);
                        }
                    )
                    .WithName(nameof(GetProductPassport))
                    .WithTags(nameof(Product))
                    .WithOpenApi(o =>
                    {
                        o.Summary = "Get a product passport by id (Latin name + location details)";
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
                // Fetch LatinName, Country, City and GreenhouseNumber from database
                var productData = await _context.Products
                    .AsNoTracking()
                    .Where(p => p.Id == request.Id)
                    .Select(p => new
                    {
                        p.LatinName,
                        p.Country,
                        p.City,
                        p.CCode,
                        p.GreenhouseNumber
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (productData == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                var response = new Response(
                    LatinName: productData.LatinName,
                    GardeningIdNumber: GardeningIdNumber,
                    CodeForPassportC: productData.CCode ?? string.Empty,
                    Country: productData.Country ?? string.Empty,
                    City: productData.City ?? string.Empty,
                    GreenhouseNumber: productData.GreenhouseNumber ?? 0
                );

                return Result.Ok(response);
            }
        }

        public record Response(
            string LatinName,
            string GardeningIdNumber,
            string CodeForPassportC,
            string Country,
            string City,
            int GreenhouseNumber
            );
    }
}
