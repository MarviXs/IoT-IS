namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItemContainer
{
    public int Id { get; set; }
    public required int OrderId { get; set; }
    public required string Name { get; set; }
    public required int Quantity { get; set; }
    public decimal? PricePerContainer { get; set; }
    public decimal? TotalPrice { get; set; }

    // Naviazané produkty
    public ICollection<Product> Products { get; set; } = [];
    public required Order Order { get; set; } // Add this property
}
