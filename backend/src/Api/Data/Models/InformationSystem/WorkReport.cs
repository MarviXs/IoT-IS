namespace Fei.Is.Api.Data.Models.InformationSystem;

public class WorkReport
{
    public int Id { get; set; }
    public string ReportNumber { get; set; }
    public DateTime ReportDate { get; set; }

    // Foreign Keys
    public int SupplierId { get; set; }
    public int CustomerId { get; set; }

    // Many-to-One: WorkReport belongs to one Company (Supplier)
    public Company Supplier { get; set; }

    // Many-to-One: WorkReport belongs to one Company (Customer)
    public Company Customer { get; set; }

    // One-to-Many: WorkReport has many WorkDayDetails
    public List<WorkDayDetail> WorkDayDetails { get; set; } = new List<WorkDayDetail>();
}
