namespace Fei.Is.Api.Data.Models.InformationSystem;

public class InvoiceItem : BaseModel
{
    public string PluCode { get; set; }
    public int Quantity { get; set; }
    public string ItemDescription { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
