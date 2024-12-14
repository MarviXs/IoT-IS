namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Invoice : BaseModel
{
    public string InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }

    public Company Supplier { get; set; }
    public Company Customer { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
}
