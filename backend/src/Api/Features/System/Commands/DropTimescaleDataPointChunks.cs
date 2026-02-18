using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Fei.Is.Api.Features.System.Commands;

public static class DropTimescaleDataPointChunks
{
    public record Request(DateTimeOffset? From, DateTimeOffset? To);

    public record Response(int DroppedChunkCount, IReadOnlyCollection<string> DroppedChunks);

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "system/storage/chunks/drop",
                    async Task<Results<Ok<Response>, ValidationProblem>> (
                        IMediator mediator,
                        Request request,
                        CancellationToken cancellationToken
                    ) =>
                    {
                        var result = await mediator.Send(new Command(request), cancellationToken);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .RequireAuthorization("Admin")
                .WithName(nameof(DropTimescaleDataPointChunks))
                .WithTags("System")
                .WithOpenApi(o =>
                {
                    o.Summary = "Drop TimescaleDB datapoint chunks";
                    o.Description = "Drops full datapoint hypertable chunks in the requested time window for fast deletions.";
                    return o;
                });
        }
    }

    public record Command(Request Request) : IRequest<Result<Response>>;

    public sealed class Handler(TimeScaleDbContext timescaleContext, IValidator<Command> validator) : IRequestHandler<Command, Result<Response>>
    {
        private const string DropChunksOlderThanSql = """
            SELECT drop_chunks(relation => '"DataPoints"', older_than => @older_than) AS "Value";
            """;

        private const string DropChunksNewerThanSql = """
            SELECT drop_chunks(relation => '"DataPoints"', newer_than => @newer_than) AS "Value";
            """;

        private const string DropChunksBetweenSql = """
            SELECT drop_chunks(relation => '"DataPoints"', older_than => @older_than, newer_than => @newer_than) AS "Value";
            """;

        public async Task<Result<Response>> Handle(Command message, CancellationToken cancellationToken)
        {
            var validationResult = validator.Validate(message);
            if (!validationResult.IsValid)
            {
                return Result.Fail(new ValidationError(validationResult));
            }

            var droppedChunks = await DropChunksAsync(message.Request.From, message.Request.To, cancellationToken);
            return Result.Ok(new Response(droppedChunks.Count, droppedChunks));
        }

        private async Task<List<string>> DropChunksAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken)
        {
            if (from.HasValue && to.HasValue)
            {
                var parameters = new[]
                {
                    new NpgsqlParameter("older_than", NpgsqlDbType.TimestampTz) { Value = to.Value },
                    new NpgsqlParameter("newer_than", NpgsqlDbType.TimestampTz) { Value = from.Value }
                };
                return await timescaleContext.Database.SqlQueryRaw<string>(DropChunksBetweenSql, parameters).ToListAsync(cancellationToken);
            }

            if (to.HasValue)
            {
                var parameters = new[] { new NpgsqlParameter("older_than", NpgsqlDbType.TimestampTz) { Value = to.Value } };
                return await timescaleContext.Database.SqlQueryRaw<string>(DropChunksOlderThanSql, parameters).ToListAsync(cancellationToken);
            }

            var newerThanParameters = new[] { new NpgsqlParameter("newer_than", NpgsqlDbType.TimestampTz) { Value = from!.Value } };
            return await timescaleContext.Database.SqlQueryRaw<string>(DropChunksNewerThanSql, newerThanParameters).ToListAsync(cancellationToken);
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Request)
                .Must(request => request.From.HasValue || request.To.HasValue)
                .WithMessage("At least one boundary must be provided.");

            RuleFor(command => command.Request)
                .Must(request => !request.From.HasValue || !request.To.HasValue || request.From.Value < request.To.Value)
                .WithMessage("From must be earlier than To.");
        }
    }
}
