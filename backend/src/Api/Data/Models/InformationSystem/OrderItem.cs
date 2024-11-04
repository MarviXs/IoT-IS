namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Guid ProductNumber { get; set; }
    public string VarietyName { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; }
    public Product Product { get; set; }
}
