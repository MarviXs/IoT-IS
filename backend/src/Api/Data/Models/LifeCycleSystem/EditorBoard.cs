namespace Fei.Is.Api.Data.Models.LifeCycleSystem
{
    public class EditorBoard : BaseModel
    {
        public string EditorBoardID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public string Shape { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public ICollection<EditorPlant> Plants { get; set; } = new List<EditorPlant>();
        public string Label => $"{Columns} stĺpcov x {Rows} riadkov ({Width}x{Height})";
        public string GreenHouseId { get; set; }
        public GreenHouse GreenHouse { get; set; }
    }
}
