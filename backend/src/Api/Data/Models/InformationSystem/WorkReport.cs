namespace Fei.Is.Api.Data.Models.InformationSystem;

public class WorkReport: BaseModel
{
    public int Id { get; set; }
    public string ReportNumber { get; set; }
    public DateTime ReportDate { get; set; }
    //public int SupplierId { get; set; }
    //public int CustomerId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid SupplierId { get; set; }

    public Company Supplier { get; set; }
    public Company Customer { get; set; }
    public List<WorkDayDetail> WorkDayDetails { get; set; } = new List<WorkDayDetail>();
}
