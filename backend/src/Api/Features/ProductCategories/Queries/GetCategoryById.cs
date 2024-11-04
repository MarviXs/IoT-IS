using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.ProductCategories.Queries;

public static class GetCategoryById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "product-categories/{id:Guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, Guid id) =>
                    {
                        var query = new Query(id);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetCategoryById))
                .WithTags(nameof(Category))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a category by Id";
                    return o;
                });
        }
    }

    public record Query(Guid Id) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == request.Id, cancellationToken);

            if (category == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var response = new Response(
                Id: category.Id,
                Name: category.CategoryName
            );

            return Result.Ok(response);
        }
    }

    public record Response(
        Guid Id,
        string Name
    );
}
