using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using Fei.Is.Api.Data.Enums;
using Microsoft.Extensions.Logging;

namespace Fei.Is.Api.Services.Notifications;

public class DiscordNotificationService(HttpClient httpClient, ILogger<DiscordNotificationService> logger) : IDiscordNotificationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<NotificationSeverity, int> SeverityColors = new Dictionary<NotificationSeverity, int>
    {
        [NotificationSeverity.Info] = 0x3B82F6,
        [NotificationSeverity.Warning] = 0xF59E0B,
        [NotificationSeverity.Serious] = 0xEF4444,
        [NotificationSeverity.Critical] = 0x7F1D1D,
    };

    private sealed record DiscordWebhookPayload(IReadOnlyList<DiscordEmbed> Embeds);

    private sealed record DiscordEmbed(string Title, string Description, int Color, string Timestamp, IReadOnlyList<DiscordEmbedField> Fields);

    private sealed record DiscordEmbedField(string Name, string Value, bool Inline = false);

    public async Task SendAsync(
        string webhookUrl,
        string title,
        string description,
        IReadOnlyCollection<(string Name, string Value)> fields,
        NotificationSeverity severity,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogWarning("Discord webhook URL is missing. Notification will not be sent.");
            return;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            logger.LogWarning("Discord notification message is missing. Notification will not be sent.");
            return;
        }

        try
        {
            var embedFields = fields
                .Where(field => !string.IsNullOrWhiteSpace(field.Name) && !string.IsNullOrWhiteSpace(field.Value))
                .Select(field => new DiscordEmbedField(field.Name, field.Value))
                .ToArray();

            var embed = new DiscordEmbed(
                title,
                description,
                SeverityColors.TryGetValue(severity, out var color) ? color : 0x5865F2,
                DateTimeOffset.UtcNow.ToString("O"),
                embedFields
            );

            var payload = new DiscordWebhookPayload([embed]);
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
