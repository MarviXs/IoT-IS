using System.ComponentModel.DataAnnotations.Schema;

namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Order : BaseModel
{
    public string? Note { get; set; }
    public DateTime OrderDate { get; set; }
    public int DeliveryWeek { get; set; }
    public string PaymentMethod { get; set; }
    public string ContactPhone { get; set; }
    public Company Customer { get; set; }

    [NotMapped]
    public decimal? TotalPrice
    {
        get { return ItemContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum(); }
        set { }
    }
    public ICollection<OrderItemContainer> ItemContainers { get; set; } = [];
}
