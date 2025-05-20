using NPOI.HPSF;

namespace Fei.Is.Api.Data.Models.LifeCycleSystem;

public class Plant : BaseModel
{
    public required Guid PlantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime DatePlanted { get; set; }
    public ICollection<PlantAnalysis> Analyses { get; set; } = [];
    public Guid PlantBoardId { get; set; } = Guid.Empty!;
    public PlantBoard? PlantBoard { get; set; }
}
