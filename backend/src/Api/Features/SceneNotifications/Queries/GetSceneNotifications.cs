using System.Security.Claims;
using Carter;
using Fei.Is.Api.Common.Errors;
using Fei.Is.Api.Common.Pagination;
using Fei.Is.Api.Common.Utils;
using Fei.Is.Api.Data.Contexts;
using Fei.Is.Api.Data.Enums;
using Fei.Is.Api.Data.Models;
using Fei.Is.Api.Extensions;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Fei.Is.Api.Features.Notifications.Queries;

public static class GetSceneNotifications
{
    public class QueryParameters : SearchParameters { }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "scene-notifications",
                    async Task<Results<Ok<PagedList<Response>>, ValidationProblem>> (
                        IMediator mediator,
                        ClaimsPrincipal user,
                        [AsParameters] QueryParameters paramaters
                    ) =>
                    {
                        var query = new Query(user, paramaters);
                        var result = await mediator.Send(query);

                        if (result.HasError<ValidationError>())
                        {
                            return TypedResults.ValidationProblem(result.ToValidationErrors());
                        }

                        return TypedResults.Ok(result.Value);
                    }
                )
                .WithName(nameof(GetSceneNotifications))
                .WithTags(nameof(SceneNotification))
                .WithOpenApi(o =>
                {
                    o.Summary = "Get paginated scene notifications";
                    return o;
                });
        }
    }

    public record Query(ClaimsPrincipal User, QueryParameters Parameters) : IRequest<Result<PagedList<Response>>>;

    public sealed class Handler(AppDbContext context, IValidator<QueryParameters> validator) : IRequestHandler<Query, Result<PagedList<Response>>>
    {
        public async Task<Result<PagedList<Response>>> Handle(Query message, CancellationToken cancellationToken)
        {
            var queryParameters = message.Parameters;

            var result = validator.Validate(queryParameters);
            if (!result.IsValid)
            {
                return Result.Fail(new ValidationError(result));
            }

            var query = context
                .SceneNotifications.AsNoTracking()
                .Include(s => s.Scene)
                .Where(s => s.Scene!.OwnerId == message.User.GetUserId())
                .Sort(queryParameters.SortBy ?? nameof(SceneNotification.CreatedAt), queryParameters.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            var notifications = await query
                .Paginate(queryParameters)
                .Select(notification => new Response(notification.Id, notification.SceneId, notification.Scene != null ? notification.Scene.Name : null, notification.Message, notification.Severity, notification.CreatedAt))
                .ToListAsync(cancellationToken);

            return Result.Ok(notifications.ToPagedList(totalCount, queryParameters.PageNumber, queryParameters.PageSize));
        }
    }

    public record Response(Guid Id, Guid SceneId, string? SceneName, string Message, NotificationSeverity Severity, DateTime CreatedAt);

    public sealed class ParametersValidator : AbstractValidator<QueryParameters>
    {
        private static readonly string[] ValidSortByFields = [nameof(SceneNotification.CreatedAt), nameof(SceneNotification.Severity), nameof(SceneNotification.Message)];

        public ParametersValidator()
        {
            RuleFor(x => x.SortBy).ValidSortBy(ValidSortByFields);
        }
    }
}
