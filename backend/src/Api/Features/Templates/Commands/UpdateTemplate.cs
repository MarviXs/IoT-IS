using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.Extensions;
using Fei.Is.Api.FileSystem;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Templates.Commands;

public static class UpdateTemplate
{
    public record Request(IFormFile File, FileIdentifier Identifier);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "templates",
                    async Task<Results<NotFound, ValidationProblem, Ok>> (IMediator mediator, ClaimsPrincipal user, [FromForm] Request request) =>
                    {
                        var command = new Command(request, user);
                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                            return TypedResults.ValidationProblem(result.ToValidationErrors());

                        if (result.IsFailed && result.HasError<NotFoundError>())
                            return TypedResults.NotFound();

                        return TypedResults.Ok();
                    }
                )
                .DisableAntiforgery()
                .WithName(nameof(UpdateTemplate))
                .WithTags(nameof(UserFile))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update an document template";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator, IFileSystemService fileSystemService) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            FileIdentifier fileIdentifier = Enum.TryParse<FileIdentifier>(message.Request.Identifier.ToString(), out var identifier)
                ? identifier
                : throw new ArgumentException("Invalid file identifier");

            var templateQuery = context.UserFiles.Where(file => file.FileIdentifier == fileIdentifier);
            if (await templateQuery.AnyAsync(cancellationToken))
            {
                UserFile templateIdentifier = await templateQuery.FirstAsync(cancellationToken);

                templateIdentifier.OriginalFileName = message.Request.File.FileName;

                templateIdentifier.LocalFileName = fileSystemService.SaveFile(message.Request.File, templateIdentifier);
            }
            else
            {
                var template = new UserFile
                {
                    FileIdentifier = fileIdentifier,
                    LocalFileName = null,
                    OriginalFileName = message.Request.File.FileName,
                };

                template.LocalFileName = fileSystemService.SaveFile(message.Request.File, template);

                context.UserFiles.Add(template);
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Request.File).NotEmpty().WithMessage("File is required");
            RuleFor(r => r.Request.Identifier).IsInEnum().WithMessage("Template identifier is required");
        }
    }
}
