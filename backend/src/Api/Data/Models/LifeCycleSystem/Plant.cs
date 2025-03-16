namespace Fei.Is.Api.Data.Models.LifeCycleSystem;

public class Plant : BaseModel
    {
        public string PlantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime DatePlanted { get; set; }
        public ICollection<PlantAnalysis> Analyses { get; set; } = new List<PlantAnalysis>();
        public String PlantBoardId { get; set; }
        public PlantBoard PlantBoard { get; set; }
    }