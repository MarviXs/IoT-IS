namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItem : BaseModel
{
    public required Product Product { get; set; } // Objekt Product uložený priamo v OrderItem

    public required int Quantity { get; set; } // Počet kusov daného produktu
}
