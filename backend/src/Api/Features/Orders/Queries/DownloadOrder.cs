using System.Security.Claims;
using Carter;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models.InformationSystem;
using MediatR;

namespace Fei.Is.Api.Features.Orders.Queries;

public static class DownloadOrder
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "orders/{id:Guid}/download",
                    async (IMediator mediator, ClaimsPrincipal user, HttpContext context, Guid id) =>
                    {
                        var query = new Query(user);
                        var result = await mediator.Send(query);

                        context.Response.ContentType = "application/octet-stream";
                        await result.CopyToAsync(context.Response.Body);
                        result.Close();
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

    public record Query(ClaimsPrincipal User) : IRequest<FileStream>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, FileStream>
    {
        public async Task<FileStream> Handle(Query message, CancellationToken cancellationToken)
        {
            return new FileStream("C:\\Users\\Jakub\\Downloads\\aa.xls", FileMode.Open, FileAccess.Read);
        }
    }
}
