namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }

    public int SupplierId { get; set; }
    public int CustomerId { get; set; }

    public Company Supplier { get; set; }
    public Company Customer { get; set; }
    public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
