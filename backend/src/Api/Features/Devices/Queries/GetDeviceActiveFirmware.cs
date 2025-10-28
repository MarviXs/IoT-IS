using System.Linq;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Devices.Queries;

public static class GetDeviceActiveFirmware
{
    public record Request(string AccessToken);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "devices/firmwares/active",
                    async Task<Results<Ok<Response>, ValidationProblem, NotFound>> (IMediator mediator, Request request) =>
                    {
                        var query = new Query(request);
                        var result = await mediator.Send(query);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }
                        if (result.HasError<NotFoundError>())
                        {
                            return TypedResults.NotFound();
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .AllowAnonymous()
                .WithName(nameof(GetDeviceActiveFirmware))
                .WithTags(nameof(Device))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get the active firmware for a device using its access token";
                    return o;
                });
        }
    }

    public record Query(Request Request) : IRequest<Result<Response>>;

    public sealed class Handler(AppDbContext context, IValidator<Query> validator) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var device = await context
                .Devices.AsNoTracking()
                .Include(d => d.DeviceTemplate)
                .ThenInclude(t => t.Firmwares)
                .FirstOrDefaultAsync(d => d.AccessToken == message.Request.AccessToken, cancellationToken);

            if (device == null)
            {
                return Result.Fail(new NotFoundError());
            }

            if (device.DeviceTemplate == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var activeFirmware = device.DeviceTemplate.Firmwares.FirstOrDefault(f => f.IsActive);
            if (activeFirmware == null)
            {
                return Result.Fail(new NotFoundError());
            }

            var downloadUrl = $"/api/devices/firmwares/{activeFirmware.Id}/download?accessToken={Uri.EscapeDataString(message.Request.AccessToken)}";

            var response = new Response(
                activeFirmware.Id,
                activeFirmware.VersionNumber,
                activeFirmware.OriginalFileName,
                downloadUrl
            );

            return Result.Ok(response);
        }
    }

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(q => q.Request.AccessToken).NotEmpty().WithMessage("Access token is required");
        }
    }

    public record Response(Guid FirmwareId, string VersionNumber, string OriginalFileName, string DownloadUrl);
}
