using System.IO;
using System.Net.Mime;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.FileSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Stubble.Core.Contexts;

namespace Fei.Is.Api.Features.Templates.Queries;

public static class DownloadTemplateById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "templates/download",
                    async Task<Results<NotFound, FileStreamHttpResult>> (IMediator mediator, ClaimsPrincipal user, HttpContext context, FileIdentifier identifier) =>
                    {
                        var query = new Query(user, identifier);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        FileStream fileStream = result.Value.FileStream;

                        ContentDisposition contentDisposition = new ContentDisposition
                        {
                            FileName = result.Value.FileName,
                            Inline = false
                        };
                        context.Response.Headers.ContentDisposition = contentDisposition.ToString();
                        context.Response.Headers.AccessControlExposeHeaders = "Content-Disposition";

                        context.Response.ContentType = "application/octet-stream";

                        return TypedResults.File(fileStream);
                    }
                )
                .WithName(nameof(DownloadTemplateById))
                .WithTags(nameof(UserFile))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get template by Identifier";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, FileIdentifier identifier) : IRequest<Result<Response>>;
    public sealed class Handler(AppDbContext context, IFileSystemService fileSystemService) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            FileIdentifier identifier = message.identifier;

            var query = context
                .UserFiles.AsNoTracking()
                .Where(file => file.FileIdentifier == identifier);

            if (!await query.AnyAsync())
            {
                return Result.Fail(new NotFoundError());
            }

            var file = await query.FirstAsync(cancellationToken);
            return new Response(fileSystemService.GetFile(file), file.OriginalFileName);
        }
    }

    public record Response(
        FileStream FileStream,
        string FileName
    );
}
