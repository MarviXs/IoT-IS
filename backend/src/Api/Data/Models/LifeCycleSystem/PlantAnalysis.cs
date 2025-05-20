namespace Fei.Is.Api.Data.Models.LifeCycleSystem;

public class PlantAnalysis : BaseModel
{
    public Guid RecordId { get; set; }
    public Guid? PlantId { get; set; }
    public Plant Plant { get; set; } = null!;
    public DateTime AnalysisDate { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    public int LeafCount { get; set; }
    public double Area { get; set; }
    public string Disease { get; set; } = string.Empty;
    public string Health { get; set; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
}
