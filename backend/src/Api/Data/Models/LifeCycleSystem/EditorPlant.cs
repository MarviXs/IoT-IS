namespace Fei.Is.Api.Data.Models.LifeCycleSystem
{
    public class EditorPlant : BaseModel
    {
        public string PlantID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public DateTime DateCreated { get; set; }
        public int CurrentDay { get; set; }
        public string Stage { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string PlantDetails => $"{Name} - {Type} - Day {CurrentDay} - {Stage} - {CurrentState}";
        public string EditorBoardId { get; set; }
        public string GreenHouseId { get; set; }
        public GreenHouse GreenHouse { get; set; }
    }
}