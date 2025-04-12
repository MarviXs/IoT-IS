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
    public decimal? PriceReduced
    {
        get { return ReducedTaxContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum(); }
        set { }
    }

    [NotMapped]
    public decimal? PriceNormal
    {
        get { return NormalTaxContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum(); }
        set { }
    }


    public ICollection<OrderItemContainer> ItemContainers { get; set; } = [];

    [NotMapped]
    public ICollection<OrderItemContainer> ReducedTaxContainers
    {
        get { return ItemContainers.Where(i => i.Items.All(i => i.Product.VATCategory.Name.Equals("Reduced"))).ToList(); }
        set { }
    }

    [NotMapped]
    public ICollection<OrderItemContainer> NormalTaxContainers
    {
        get { return ItemContainers.Where(i => i.Items.All(i => i.Product.VATCategory.Name.Equals("Normal"))).ToList(); }
        set { }
    }

    [NotMapped]
    public decimal? TotalReducedContainersPrice
    {
        get { return ReducedTaxContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum(); }
        set { }
    }

    [NotMapped]
    public decimal? TotalNormalContainersPrice
    {
        get { return NormalTaxContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum(); }
        set { }
    }

    [NotMapped]
    public decimal? VATReduced
    {
        get
        {
            if (ReducedTaxContainers.Count == 0 || ReducedTaxContainers.First().Items.Count == 0)
                return 0;
            return ReducedTaxContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum() * (ReducedTaxContainers.First().Items.First()
                .Product.VATCategory.Rate / 100);
        }
        set { }
    }

    [NotMapped]
    public decimal? VATNormal
    {
        get
        {
            if (NormalTaxContainers.Count == 0 || NormalTaxContainers.First().Items.Count == 0)
                return 0;
            return NormalTaxContainers.Select(i => i.TotalPrice).DefaultIfEmpty(0).Sum() * (NormalTaxContainers.First().Items.First()
                .Product.VATCategory.Rate / 100);
        }
        set { }
    }
}
