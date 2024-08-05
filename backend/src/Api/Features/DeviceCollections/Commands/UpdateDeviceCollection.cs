using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fei.Is.Api.Features.DeviceCollections.Commands;

public static class UpdateDeviceCollection
{
    public record Request(string Name);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "device-collections/{id:guid}",
                    async Task<Results<Ok<Response>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request, Guid id) =>
                    {
                        var command = new Command(request, id, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        
                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(UpdateDeviceCollection))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Update a device collection";
                    return o;
                });
        }
    }

    public record Command(Request Request, Guid CollectionId, ClaimsPrincipal User) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var collection = await context.DeviceCollections.FindAsync([message.CollectionId], cancellationToken: cancellationToken);
            if (collection == null)
            {
                return Result.Fail(new NotFoundError());
            }

            collection.Name = message.Request.Name;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(new Response(collection.Id, collection.Name));
        }
    }

    public record Response(Guid Id, string Name);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.Name).NotEmpty();
        }
    }
}
