namespace Fei.Is.Api.Data.Models.LifeCycleSystem;

public class GreenHouse : BaseModel
{
    public required Guid GreenHouseID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Depth { get; set; }
    public DateTime DateCreated { get; set; }
    public ICollection<EditorBoard> Pots { get; set; } = new List<EditorBoard>();
    public ICollection<EditorPlant> Plants { get; set; } = new List<EditorPlant>();
}
