namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItem : BaseModel
{
    public required Product Product { get; set; }

    public required int Quantity { get; set; }
}
