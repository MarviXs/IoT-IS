namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItemContainer : BaseModel
{
    public required string Name { get; set; }
    public required int Quantity { get; set; }
    public decimal? PricePerContainer { get; set; }
    public decimal? TotalPrice { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
}
