using Carter;
using Fei.Is.Api.Data.Contexts;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Auth.Commands;

public static class AuthDevice
{
    public sealed class Endpoint : ICarterModule
    {
        public record Request(string AccessToken);

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "auth/device",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (Request request, IMediator mediator) =>
                    {
                        var query = new Query(request.AccessToken);
                        var result = await mediator.Send(query);
                        return TypedResults.Ok(result.Value);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(AuthDevice))
                .WithTags(nameof(Auth))
                .WithOpenApi(o =>
                {
                    o.Summary = "This endpoint endpoint is called by EMQX to authenticate a device";
                    return o;
                });
        }
    }

    public record Query(string AccessToken) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var device = await context
                .Devices.AsNoTracking()
                .Where(device => device.AccessToken == request.AccessToken)
                .FirstOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                return Result.Ok(new Response("ignore", false));
            }

            var response = new Response("allow", false, new Dictionary<string, string>
            {
                { "deviceId", device.Id.ToString() },
                { "deviceName", device.Name },
            });

            return Result.Ok(response);
        }
    }

    public record Response(string Result, bool Is_superuser, Dictionary<string, string>? Client_attrs = null);
}
