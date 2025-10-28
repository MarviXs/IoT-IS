using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Services.Notifications;

public interface IDiscordNotificationService
{
    Task SendAsync(
        string webhookUrl,
        string title,
        string description,
        IReadOnlyCollection<(string Name, string Value)> fields,
        NotificationSeverity severity,
        CancellationToken cancellationToken = default
    );
}
