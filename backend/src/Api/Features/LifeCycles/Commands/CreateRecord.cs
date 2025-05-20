using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.LifeCycleSystem;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.LifeCycles.Commands;

public static class CreateRecord
{
    public record Request(
        Guid PlantId,
        DateTime AnalysisDate,
        double Height,
        double Width,
        int LeafCount,
        double Area,
        string Disease,
        string Health,
        string ImageName
    );

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "lifecycles",
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        var command = new Command(request, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateRecord))
                .WithTags(nameof(PlantAnalysis))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a plant analysis record";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = await validator.ValidateAsync(message, cancellationToken);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            if (!context.Plants.Any(p => p.Id == message.Request.PlantId))
            {
                return Result.Fail(new BadRequestError("Product with this PlantId doesnt exist"));
            }

            var plantAnalysis = new PlantAnalysis
            {
                PlantId = message.Request.PlantId,
                Height = message.Request.Height,
                Width = message.Request.Width,
                LeafCount = message.Request.LeafCount,
                Area = message.Request.Area,
                Disease = message.Request.Disease,
                Health = message.Request.Health,
                AnalysisDate = message.Request.AnalysisDate,
                ImageName = message.Request.ImageName,
            };

            await context.PlantAnalyses.AddAsync(plantAnalysis, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(plantAnalysis.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.PlantId).NotEmpty().WithMessage("PlantId is required");
            RuleFor(r => r.Request.AnalysisDate).NotEmpty().WithMessage("Analysis date is required");
        }
    }
}
