using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Jobs.Queries;

public static class GetJobsOnDevice
{
    public class QueryParameters : SearchParameters
    {
        public Guid? DeviceId { get; set; }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "jobs",
                    async Task<Results<Ok<PagedList<Response>>, NotFound, ForbidHttpResult>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters parameters
                    ) =>
                    {
                        var query = new Query(user, parameters);
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
                .WithName(nameof(GetJobsOnDevice))
                .WithTags(nameof(Job))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get all jobs on a device";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters queryParameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, IValidator<QueryParameters> validator) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var result = validator.Validate(message.queryParameters);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var query = context.Jobs.AsNoTracking().Include(j => j.Device).Where(j => j.Device!.OwnerId == message.User.GetUserId());

            if (message.queryParameters.SortBy == "DeviceName")
            {
                query = query.Sort(j => j.Device!.Name, message.queryParameters.Descending);
            }
            else if (message.queryParameters.SortBy == nameof(Job.Status))
            {
                query = query.Sort(message.queryParameters.SortBy ?? nameof(Job.StartedAt), message.queryParameters.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var jobs = await query
                .Paginate(message.queryParameters)
                .Select(job => new Response(
                    new DeviceResponse(job.Device!.Id, job.Device.Name),
                    job.Id,
                    job.Name,
                    job.Status,
                    job.CurrentStep,
                    job.TotalSteps,
                    job.CurrentCycle,
                    job.TotalCycles,
                    job.Paused,
                    job.StartedAt,
                    job.FinishedAt,
                    job.CreatedAt,
                    job.UpdatedAt
                ))
                .ToListAsync(cancellationToken);

            return jobs.ToPagedList(totalCount, message.queryParameters.PageNumber, message.queryParameters.PageSize);
        }
    }

    public record JobStatusResponse(JobStatusEnum RetCode, JobStatusEnum Code, int CurrentStep, int TotalSteps, int CurrentCycle);

    public record DeviceResponse(Guid Id, string Name);

    public record Response(
        DeviceResponse Device,
        Guid Id,
        string Name,
        JobStatusEnum Status,
        int CurrentStep = 1,
        int TotalSteps = 1,
        int CurrentCycle = 1,
        int TotalCycles = 1,
        bool Paused = false,
        DateTime? StartedAt = null,
        DateTime? FinishedAt = null,
        DateTime? CreatedAt = null,
        DateTime? UpdatedAt = null
    );

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields =
        [
            nameof(Job.Name),
            nameof(Job.CreatedAt),
            nameof(Job.FinishedAt),
            nameof(Job.UpdatedAt),
            nameof(Job.Status),
            "DeviceName"
        ];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
