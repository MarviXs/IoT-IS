namespace Fei.Is.Api.Data.Models.InformationSystem;

public class DeliveryItem : BaseModel
{
    public required string PluCode { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; }
    public string PlantPassport { get; set; }
    public string PackSize { get; set; }
    public decimal UnitPriceWithoutVat { get; set; }
    public decimal TotalPriceWithoutVat { get; set; }
    public decimal VatRate { get; set; }
}
