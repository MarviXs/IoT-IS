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

namespace Fei.Is.Api.Features.Products.Commands;

public static class UpdateProduct
{
    public record Request(
        string PLUCode,
        string Code,
        string LatinName,
        string? CzechName,
        string? FlowerLeafDescription,
        string? PotDiameterPack,
        decimal? pricePerPiecePack,
        decimal? pricePerPiecePackVAT,
        decimal? discountedPriceWithoutVAT,
        decimal? RetailPrice,
        int CategoryId
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "products/{id:PLUCode}",
                    async Task<Results<NotFound, ValidationProblem, NoContent>> (IMediator mediator, String pluCode, Request request) =>
                    {
                        var command = new Command(request, pluCode);

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
                .WithName(nameof(UpdateProduct))
                .WithTags(nameof(Product))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a product";
                    return o;
                });
        }
    }

    public record Command(Request Request, String PLUCode) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var product = await context.Products.FindAsync([message.PLUCode], cancellationToken);
            if (product == null)
            {
                return Result.Fail(new NotFoundError());
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public record Response(Guid Id, string Name, long? ResponseTime, long? LastResponseTimestamp);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.PLUCode).NotEmpty().WithMessage("PLUCode is required");
            RuleFor(r => r.Request.LatinName).NotEmpty().WithMessage("LatinName is required");
            RuleFor(r => r.Request.CategoryId).NotEmpty().WithMessage("CategoryId is required");
        }
    }
}
