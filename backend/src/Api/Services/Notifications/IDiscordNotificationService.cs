namespace Fei.Is.Api.Services.Notifications;

public interface IDiscordNotificationService
{
    Task SendAsync(string webhookUrl, string message, CancellationToken cancellationToken = default);
}
