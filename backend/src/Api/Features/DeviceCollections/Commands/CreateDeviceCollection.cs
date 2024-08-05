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

public static class CreateDeviceCollection
{
    public record Request(string Name, Guid? CollectionParentId);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "device-collections",
                    async Task<Results<Created<Guid>, ValidationProblem>> (IMediator mediator, ClaimsPrincipal user, Request request) =>
                    {
                        var command = new Command(request, user);

                        var result = await mediator.Send(command);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Created(result.Value.ToString(), result.Value);
                    }
                )
                .WithName(nameof(CreateDeviceCollection))
                .WithTags(nameof(DeviceCollection))
                .WithOpenApi(o =>
                {
                    o.Summary = "Create a device collection";
                    return o;
                });
        }
    }

    public record Command(Request Request, ClaimsPrincipal User) : IRequest<Result<Guid>>;

    public sealed class Handler(AppDbContext context, IValidator<Command> validator) : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var deviceCollection = new DeviceCollection
            {
                Name = message.Request.Name,
                OwnerId = message.User.GetUserId(),
                IsRoot = true
            };

            if (message.Request.CollectionParentId.HasValue)
            {
                deviceCollection.IsRoot = false;
                var parentCollection = await context.DeviceCollections.FindAsync(
                    [message.Request.CollectionParentId],
                    cancellationToken: cancellationToken
                );
                if (parentCollection == null)
                {
                    return Result.Fail(new NotFoundError());
                }

                deviceCollection.SubItems.Add(new CollectionItem { CollectionParent = parentCollection });
            }

            await context.DeviceCollections.AddAsync(deviceCollection, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok(deviceCollection.Id);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request.Name).NotEmpty();
        }
    }
}
