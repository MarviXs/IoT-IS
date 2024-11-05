namespace Fei.Is.Api.Data.Models;

public class Scene : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty;
    public ApplicationUser? Owner { get; set; } = null!;
    public required string Name { get; set; }
    public bool IsEnabled { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public List<SceneAction> Actions { get; set; } = [];
}
