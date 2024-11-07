namespace Fei.Is.Api.Data.Models;

public class SceneSensorTrigger : BaseModel
{
    public Guid SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public required string SensorTag { get; set; }
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}
