using System.Net.Http.Json;
using System.Text.Json;
using Fei.Is.Api.Data.Enums;
using Microsoft.Extensions.Logging;

namespace Fei.Is.Api.Services.Notifications;

public class DiscordNotificationService(HttpClient httpClient, ILogger<DiscordNotificationService> logger) : IDiscordNotificationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private sealed record DiscordWebhookPayload(string Content, NotificationSeverity Severity);

    public async Task SendAsync(string webhookUrl, string message, NotificationSeverity severity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogWarning("Discord webhook URL is missing. Notification will not be sent.");
            return;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Discord notification message is missing. Notification will not be sent.");
            return;
        }

        try
        {
            var payload = new DiscordWebhookPayload(message, severity);
            var response = await httpClient.PostAsJsonAsync(webhookUrl, payload, SerializerOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Discord webhook responded with a non-success status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to send Discord notification.");
        }
    }
}
