using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class Command : BaseModel
{
    public required Guid DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }

    public required string DisplayName { get; set; }
    public required string Name { get; set; }

    public List<double> Params { get; set; } = [];
    public List<RecipeStep> RecipeSteps { get; set; } = [];
}
