namespace Fei.Is.Api.Data.Models.InformationSystem;

public class WorkDayDetail : BaseModel
{
    public DateTime WorkDate { get; set; }
    public int TaskNumber { get; set; }
    public string WorkLocation { get; set; }
    public string WorkType { get; set; }
    public int WorkersCount { get; set; }
    public int WorkHours { get; set; }
    public decimal RateA { get; set; }
    public decimal RateB { get; set; }
    public decimal TotalA { get; set; }
    public decimal TotalB { get; set; }
    public string Equipment { get; set; }
    public decimal TotalEquipment { get; set; }
}
