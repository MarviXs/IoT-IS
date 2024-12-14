namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItem : BaseModel
{
    public int Id { get; set; } // ID objednávkovej položky
    public required int OrderId { get; set; } // ID objednávky
    public required int ContainerId { get; set; } // ID kontajnera

    // Produkt priamo ako objekt
    public required Product Product { get; set; } // Objekt Product uložený priamo v OrderItem

    // Atribúty mimo produktu
    public required int Quantity { get; set; } // Počet kusov daného produktu
    // Navigačné vlastnosti
    public required Order Order { get; set; } // Navigácia na objednávku

    public required OrderItemContainer Container { get; set; } // Navigácia na kontajner
}