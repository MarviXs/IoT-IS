using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class Recipe : BaseModel
{
    public required string Name { get; set; }
    public required Guid DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }
    public List<RecipeStep> Steps { get; set; } = [];
    public List<RecipeStep> ParentSteps { get; set; } = [];
}
