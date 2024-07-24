using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Commands.Queries;

public static class GetCommandById
{
    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "commands/{id:guid}",
                    async Task<Results<Ok<Response>, NotFound, ForbidHttpResult>> (IMediator mediator, ClaimsPrincipal user, Guid id) =>
                    {
                        var query = new Query(user, id);
                        var result = await mediator.Send(query);

                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }
                        if (result.HasError<ForbiddenError>())
                        {
                            return TypedResults.Forbid();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetCommandById))
                .WithTags(nameof(Command))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get a command by id";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, Guid CommandId) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var commandbyid = await context
                .Commands.Where(command => command.Id == message.CommandId)
                .Select(command => new Response(command.Id, command.DisplayName, command.Name, command.Params))
                .FirstOrDefaultAsync(cancellationToken);

            if (commandbyid == null)
            {
                return Result.Fail(new NotFoundError());
            }

            return Result.Ok(commandbyid);
        }
    }

    public record Response(Guid Id, string DisplayName, string Name, List<double> Params);
}
