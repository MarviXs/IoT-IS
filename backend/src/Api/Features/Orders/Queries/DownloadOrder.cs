using System.Net.Mime;
using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using Fei.Is.Api.DocumentsGen.Generators;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fei.Is.Api.Features.Orders.Queries;

public static class DownloadOrder
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "orders/{id:Guid}/download",
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
                .WithName(nameof(DownloadOrder))
                .WithTags(nameof(Order))
                .WithOpenApi(o =>
                {
                    o.Summary = "Download order";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid Id) : IRequest<Result<FileStream>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<FileStream>>
    {
        public async Task<Result<FileStream>> Handle(Query message, CancellationToken cancellationToken)
        {
            var order = context
                .Orders.AsNoTracking()
                .Include(o => o.Customer) // Načítanie priradenej zákazníckej entity
                .Where(o => o.Id == message.Id);

            if (!await order.AnyAsync())
            {
                return Result.Fail(new NotFoundError());
            }
            string path = "C:\\Users\\Jakub\\Downloads\\ponuka.xlsx";
            ExcelGenerator excelGenerator = new();
            List<string> fields = excelGenerator.GetFields(path);

            JToken jsonObject = JToken.FromObject(order);

            Dictionary<string, string> fieldsWithValues = new();

            foreach (var field in fields) {
                string parsedField = field.Substring(2, field.Length - 4);
                fieldsWithValues[field] = jsonObject.SelectToken("[0]." + parsedField)?.ToString() ?? String.Empty;
            }

            string retDocPath = excelGenerator.ApplyFields(path, fieldsWithValues);


            return new FileStream(retDocPath, FileMode.Open, FileAccess.Read);
        }
    }
}
