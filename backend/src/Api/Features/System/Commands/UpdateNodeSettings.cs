using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.System.Commands;

public static class UpdateNodeSettings
{
    public record Request(SystemNodeType NodeType, string? HubUrl, string? HubToken);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "system/node-settings",
                    async Task<Results<NoContent, ValidationProblem>> (IMediator mediator, Request request, CancellationToken cancellationToken) =>
                    {
                        var result = await mediator.Send(new Command(request), cancellationToken);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.NoContent();
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(UpdateNodeSettings))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Update system node settings";
                    o.Description = "Updates node type and hub connection settings for edge mode.";
                    return o;
                });
        }
    }

    public record Command(Request Request) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var settings = await context.SystemNodeSettings.OrderBy(setting => setting.CreatedAt).FirstOrDefaultAsync(cancellationToken);
            if (settings == null)
            {
                settings = new SystemNodeSetting { NodeType = message.Request.NodeType };
                await context.SystemNodeSettings.AddAsync(settings, cancellationToken);
            }
            else
            {
                settings.NodeType = message.Request.NodeType;
            }

            if (message.Request.NodeType == SystemNodeType.Edge)
            {
                settings.HubUrl = NormalizeHubUrl(message.Request.HubUrl);
                settings.HubToken = message.Request.HubToken?.Trim();
            }
            else
            {
                settings.HubUrl = null;
                settings.HubToken = null;
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }

        private static string? NormalizeHubUrl(string? hubUrl)
        {
            var trimmed = hubUrl?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return trimmed;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
            {
                return trimmed;
            }

            if (!trimmed.Contains("://", StringComparison.Ordinal))
            {
                return $"https://{trimmed}";
            }

            return trimmed;
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.NodeType).IsInEnum();

            RuleFor(command => command.Request.HubUrl)
                .NotEmpty()
                .MaximumLength(1024)
                .Must(hubUrl => Uri.TryCreate(NormalizeHubUrl(hubUrl), UriKind.Absolute, out _))
                .WithMessage("Hub URL must be a valid absolute URL")
                .When(command => command.Request.NodeType == SystemNodeType.Edge);

            RuleFor(command => command.Request.HubToken)
                .NotEmpty()
                .MaximumLength(256)
                .When(command => command.Request.NodeType == SystemNodeType.Edge);
        }

        private static string? NormalizeHubUrl(string? hubUrl)
        {
            var trimmed = hubUrl?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                return trimmed;
            }

            if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
            {
                return trimmed;
            }

            if (!trimmed.Contains("://", StringComparison.Ordinal))
            {
                return $"https://{trimmed}";
            }

            return trimmed;
        }
    }
}
