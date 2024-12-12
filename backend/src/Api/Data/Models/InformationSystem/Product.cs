namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Product : BaseModel
{
    public string PLUCode { get; set; }
    public string? Code { get; set; }
    public string LatinName { get; set; }
    public string? CzechName { get; set; }
    public string? FlowerLeafDescription { get; set; }
    public string? PotDiameterPack { get; set; }
    public decimal? PricePerPiecePack { get; set; }
    public decimal? DiscountedPriceWithoutVAT { get; set; }
    public decimal? RetailPrice { get; set; }
    public Category Category { get; set; }
    public string Variety { get; set; }
    public Supplier Supplier { get; set; }
    public VATCategory VATCategory { get; set; }
}
