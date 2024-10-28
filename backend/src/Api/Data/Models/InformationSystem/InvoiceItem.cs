namespace Fei.Is.Api.Data.Models.InformationSystem;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string PluCode { get; set; }
    public int Quantity { get; set; }
    public string ItemDescription { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    public Invoice Invoice { get; set; }
}
