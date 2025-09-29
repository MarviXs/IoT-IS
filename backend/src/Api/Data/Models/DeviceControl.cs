namespace Fei.Is.Api.Data.Models;

public class DeviceControl : BaseModel
{
    public required Guid DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }

    public required string Name { get; set; }
    public required string Color { get; set; }

    public required Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public int Cycles { get; set; } = 1;
    public bool IsInfinite { get; set; }
    public int Order { get; set; }
}
