using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class SceneNotification : BaseModel
{
    public Guid SceneId { get; set; } = Guid.Empty;
    public Scene? Scene { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
}
