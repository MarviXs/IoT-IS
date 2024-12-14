namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItem : BaseModel
{
    public string VarietyName { get; set; }
    public int Quantity { get; set; }
    public Product Product { get; set; }
}
