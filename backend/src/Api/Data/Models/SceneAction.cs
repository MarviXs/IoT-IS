using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class SceneAction
{
    public SceneActionType Type { get; set; }

    // Job type
    public Guid? DeviceId { get; set; }
    public Guid? RecipeId { get; set; }

    // Notification type
    public NotificationSeverity NotificationSeverity { get; set; } = NotificationSeverity.Info;
    public string? NotificationMessage { get; set; }
}
