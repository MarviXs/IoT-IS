using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Services.Notifications;

public interface IDiscordNotificationService
{
    Task SendAsync(
        string webhookUrl,
        string message,
        NotificationSeverity severity,
        CancellationToken cancellationToken = default
    );
}
