namespace Fei.Is.Api.Data.Models;

public class Scene : BaseModel
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public List<SceneAction> Actions { get; set; } = [];
}
