namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Order: BaseModel
{
    public int Id { get; set; }
    public string Note { get; set; }
    //public int CustomerId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public int DeliveryWeek { get; set; }
    public string PaymentMethod { get; set; }
    public string ContactPhone { get; set; }
    
    public Company Customer { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
