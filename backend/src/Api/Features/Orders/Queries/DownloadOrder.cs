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
                .Include(o => o.Customer)
                .Include(o => o.ItemContainers)
                .ThenInclude(ic => ic.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.Id == message.Id);

            if (!await order.AnyAsync())
            {
                return Result.Fail(new NotFoundError());
            }

            string path = "C:\\Users\\Jakub\\Downloads\\ponuka.xlsx";
            ExcelGenerator excelGenerator = new();

            JToken? jsonObject = JToken.FromObject(order).SelectToken("[0]");

            if (jsonObject == null)
            {
                return Result.Fail(new NotFoundError());
            }

            string retDocPath = excelGenerator.ApplyFields(path, jsonObject);

            return new FileStream(retDocPath, FileMode.Open, FileAccess.Read);
        }
    }
}
