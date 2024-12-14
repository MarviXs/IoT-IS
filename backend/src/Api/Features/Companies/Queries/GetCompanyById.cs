using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Companies.Queries;

public static class GetCompanyById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "companies/{id:Guid}",
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
                .WithName(nameof(GetCompanyById))
                .WithTags(nameof(Company))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a company by Id";
                    return o;
                });
        }
    }

    public record Query(Guid Id) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Companies.AsNoTracking().Where(company => company.Id == request.Id);

            if (!await query.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            var company = await query.FirstAsync(cancellationToken);

            var response = new Response(
                Title: company.Title,
                Ic: company.Ic,
                Dic: company.Dic,
                Street: company.Street,
                Psc: company.Psc,
                City: company.City
            );

            return Result.Ok(response);
        }
    }

    public record Response(string Title, string Ic, string? Dic, string? Street, string? Psc, string? City);
}
