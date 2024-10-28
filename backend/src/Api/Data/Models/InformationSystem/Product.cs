namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Product
{
    public string PLUCode { get; set; }
    public string Code { get; set; }
    public string LatinName { get; set; }
    public string CzechName { get; set; }
    public string FlowerLeafDescription { get; set; }
    public string PotDiameterPack { get; set; }
    public decimal PricePerPiecePack { get; set; }
    public decimal PricePerPiecePackVAT { get; set; }
    public decimal DiscountedPriceWithoutVAT { get; set; }
    public decimal RetailPrice { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
