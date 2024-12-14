using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Companies.Commands;

public static class UpdateCompany
{
    public record Request(string Title, string Ic, string? Dic, string? Street, string? Psc, string? City);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "companies/{id:Guid}",
                    async Task<Results<NotFound, ValidationProblem, NoContent>> (IMediator mediator, Guid id, Request request) =>
                    {
                        var command = new Command(request, id);

                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        else if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateCompany))
                .WithTags(nameof(Company))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a company";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid Id) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var query = context.Companies.Where(c => c.Id == message.Id);

            if (!await query.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            var company = await query.FirstAsync(cancellationToken);

            company.Ic = message.Request.Ic;
            company.Dic = message.Request.Dic;
            company.Title = message.Request.Title;
            company.Street = message.Request.Street;
            company.Psc = message.Request.Psc;
            company.City = message.Request.City;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public record Response(Guid Id, string Name, long? ResponseTime, long? LastResponseTimestamp);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.Title).NotEmpty().WithMessage("Title is required");
            RuleFor(r => r.Request.Ic).NotEmpty().WithMessage("Ic is required");
        }
    }
}
