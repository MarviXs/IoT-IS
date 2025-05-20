using System.Net.Mime;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.DocumentsGen.Generators;
using Fei.Is.Api.Services.EANCode;
using Fei.Is.Api.Services.FileSystem;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fei.Is.Api.Features.Products.Queries;

public static class DownloadProductSticker
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "products/{id:Guid}/sticker",
                    async Task<Results<NotFound, FileStreamHttpResult>> (IMediator mediator, ClaimsPrincipal user, HttpContext context, Guid id) =>
                    {
                        var query = new Query(user, id);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        FileStream fileStream = result.Value;
                        ContentDisposition contentDisposition = new ContentDisposition
                        {
                            FileName = Path.GetFileName(fileStream.Name),
                            Inline = false
                        };
                        context.Response.Headers.ContentDisposition = contentDisposition.ToString();
                        context.Response.Headers.AccessControlExposeHeaders = "Content-Disposition";

                        context.Response.ContentType = "application/octet-stream";

                        return TypedResults.File(fileStream);
                    }
                )
                .WithName(nameof(DownloadProductSticker))
                .WithTags(nameof(Product))
                .WithOpenApi(o =>
                {
                    o.Summary = "Download product sticker";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid Id) : IRequest<Result<FileStream>>;

    public sealed class Handler(AppDbContext context, IFileSystemService fileSystemService, EANCodeService eanCodeService)
        : IRequestHandler<Query, Result<FileStream>>
    {
        public async Task<Result<FileStream>> Handle(Query message, CancellationToken cancellationToken)
        {
            var product = context
                .Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.VATCategory)
                .Include(p => p.Supplier)
                .Where(p => p.Id == message.Id);

            if (!await product.AnyAsync())
            {
                return Result.Fail(new NotFoundError());
            }

            var fileQuery = context.UserFiles.Where(file => file.FileIdentifier == FileIdentifier.ProductSticker);
            if (!await fileQuery.AnyAsync(cancellationToken))
            {
                return Result.Fail(new NotFoundError());
            }

            var templateFile = await fileQuery.FirstAsync(cancellationToken);

            FileStream fileStream = fileSystemService.GetFile(templateFile);

            ExcelGenerator excelGenerator = new();

            JToken? jsonObject = JToken.FromObject(product).SelectToken("[0]");

            if (jsonObject == null)
            {
                return Result.Fail(new NotFoundError());
            }

            Dictionary<string, string> images = new();

            Product val = product.First();
            string? eanCode = eanCodeService.FromPlu(val.PLUCode);
            images.Add("barcode", eanCodeService.GenerateBarcodeImage(eanCode));

            string retDocPath = excelGenerator.ApplyFields(fileStream, templateFile.OriginalFileName, jsonObject, images);

            return new FileStream(retDocPath, FileMode.Open, FileAccess.Read);
        }
    }
}
