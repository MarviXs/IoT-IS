using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class DeviceControl : BaseModel
{
    public required Guid DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }

    public required string Name { get; set; }
    public required string Color { get; set; }

    public DeviceControlType Type { get; set; } = DeviceControlType.Run;

    public Guid? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public Guid? RecipeOnId { get; set; }
    public Recipe? RecipeOn { get; set; }

    public Guid? RecipeOffId { get; set; }
    public Recipe? RecipeOff { get; set; }

    public Guid? SensorId { get; set; }
    public Sensor? Sensor { get; set; }

    public int Cycles { get; set; } = 1;
    public bool IsInfinite { get; set; }
    public int Order { get; set; }
}
