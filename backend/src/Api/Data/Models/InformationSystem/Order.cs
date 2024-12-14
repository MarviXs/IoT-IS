namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Order : BaseModel
{
    public string Note { get; set; }
    public DateTime OrderDate { get; set; }
    public int DeliveryWeek { get; set; }
    public string PaymentMethod { get; set; }
    public string ContactPhone { get; set; }
    public Company Customer { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}
