namespace Fei.Is.Api.Data.Models;

public class SceneSensorActivator : BaseModel
{
    public Guid SceneId { get; set; }
    public Scene Scene { get; set; } = null!;

    public Guid SensorId { get; set; }
    public Sensor Sensor { get; set; } = null!;

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}
