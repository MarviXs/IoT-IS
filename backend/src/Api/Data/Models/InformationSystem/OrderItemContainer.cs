using System.ComponentModel.DataAnnotations.Schema;

namespace Fei.Is.Api.Data.Models.InformationSystem;

public class OrderItemContainer : BaseModel
{
    public required string Name { get; set; }
    public required int Quantity { get; set; }

    [NotMapped]
    public decimal? PricePerContainer
    {
        get { return Items.Select(i => i.Product.RetailPrice).DefaultIfEmpty(0).Sum() / Quantity; }
        set { }
    }

    [NotMapped]
    public decimal? TotalPrice
    {
        get { return Items.Select(i => i.Product.PricePerPiecePack * i.Quantity).DefaultIfEmpty(0).Sum(); }
        set { }
    }

    [NotMapped]
    public decimal? TotalPricePerPiece
    {
        get { return Items.Select(i => i.Product.RetailPrice).DefaultIfEmpty(0).Sum(); }
        set { }
    }
    public ICollection<OrderItem> Items { get; set; } = [];
}
