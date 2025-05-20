namespace Fei.Is.Api.Data.Models.LifeCycleSystem;

public class PlantBoard : BaseModel
{
    public Guid PlantBoardId { get; set; }
    public int Rows { get; set; }
    public int Cols { get; set; }
    public ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
