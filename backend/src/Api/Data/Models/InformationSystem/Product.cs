namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Product : BaseModel
{
    public required string PLUCode { get; set; }
    public required string Code { get; set; }
    public required string LatinName { get; set; }
    public string? Variety { get; set; }
    public string? CzechName { get; set; }
    public string? FlowerLeafDescription { get; set; }
    public string? PotDiameterPack { get; set; }
    public decimal? PricePerPiecePack { get; set; }
    public decimal? PricePerPiecePackVAT { get; set; }
    public decimal? DiscountedPriceWithoutVAT { get; set; }
    public decimal? RetailPrice { get; set; }
    public Guid CategoryId { get; set; }
    public required Category Category { get; set; }
}
