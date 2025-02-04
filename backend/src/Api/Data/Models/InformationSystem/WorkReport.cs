namespace Fei.Is.Api.Data.Models.InformationSystem;

public class WorkReport : BaseModel
{
    public string ReportNumber { get; set; }
    public DateTime ReportDate { get; set; }

    public Company Supplier { get; set; }
    public Company Customer { get; set; }
    public ICollection<WorkDayDetail> WorkDayDetails { get; set; }
}
