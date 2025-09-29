using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.DeviceControls.Commands;

public static class UpdateDeviceTemplateControls
{
    public record Request(Guid? Id, string Name, string Color, Guid RecipeId, int Cycles, bool IsInfinite);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "device-templates/{templateId:guid}/controls",
                    async Task<Results<NoContent, NotFound, ValidationProblem, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        List<Request> request,
                        Guid templateId
                    ) =>
                    {
                        var command = new Command(request, user, templateId);
                        var result = await mediator.Send(command);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .WithName(nameof(UpdateDeviceTemplateControls))
                .WithTags(nameof(DeviceControl))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update controls of a device template";
                    o.Description =
                        "Set the controls of a device template, replacing the existing controls. If Id is null, the control will be created.";
                    return o;
                });
        }
    }

    public record Command(List<Request> ControlRequest, ClaimsPrincipal User, Guid TemplateId) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var template = await context
                .DeviceTemplates.Include(t => t.Controls)
                .FirstOrDefaultAsync(t => t.Id == message.TemplateId, cancellationToken);

            if (template == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (template.OwnerId != message.User.GetUserId())
            {
                return Result.Fail(new ForbiddenError());
            }

            var validRecipeIds = await context
                .Recipes.Where(r => r.DeviceTemplateId == template.Id)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);
            var recipeLookup = validRecipeIds.ToHashSet();

            foreach (var control in message.ControlRequest)
            {
                if (!recipeLookup.Contains(control.RecipeId))
                {
                    var failure = new ValidationFailure(
                        nameof(control.RecipeId),
                        "Recipe must belong to the device template"
                    );
                    return Result.Fail(new ValidationError(new ValidationResult([failure])));
                }
            }

            context.DeviceControls.RemoveRange(template.Controls);

            for (var index = 0; index < message.ControlRequest.Count; index++)
            {
                var controlRequest = message.ControlRequest[index];
                var control = new DeviceControl
                {
                    Id = controlRequest.Id ?? Guid.NewGuid(),
                    Name = controlRequest.Name,
                    Color = controlRequest.Color,
                    RecipeId = controlRequest.RecipeId,
                    Cycles = controlRequest.Cycles,
                    IsInfinite = controlRequest.IsInfinite,
                    Order = index,
                    DeviceTemplateId = template.Id,
                };

                context.DeviceControls.Add(control);
            }

            template.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleForEach(c => c.ControlRequest)
                .ChildRules(control =>
                {
                    control.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
                    control.RuleFor(x => x.Color).NotEmpty().WithMessage("Color is required");
                    control.RuleFor(x => x.RecipeId).NotEmpty().WithMessage("Recipe ID is required");
                    control
                        .RuleFor(x => x.Cycles)
                        .GreaterThan(0)
                        .WithMessage("Cycles must be greater than 0");
                });
        }
    }
}
