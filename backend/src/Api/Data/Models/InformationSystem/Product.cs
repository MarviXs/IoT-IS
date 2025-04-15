using System.ComponentModel.DataAnnotations.Schema;

namespace Fei.Is.Api.Data.Models.InformationSystem
{
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

        public string? HeightCm { get; set; }
        public string? SeedsPerThousandPlants { get; set; }
        public string? SeedsPerThousandPots { get; set; }
        public string? SowingPeriod { get; set; }
        public string? GerminationTemperatureC { get; set; }
        public string? GerminationTimeDays { get; set; }
        public string? CultivationTimeSowingToPlant { get; set; }
        public string? SeedsMioHa { get; set; }
        public string? SeedSpacingCM { get; set; }
        public string? CultivationTimeVegetableWeek { get; set; }
        public string? BulbPlantingRequirementSqM { get; set; }
        public string? BulbPlantingPeriod { get; set; }
        public string? BulbPlantingDistanceCm { get; set; }
        public string? CultivationTimeForBulbsWeeks { get; set; }
        public string? NumberOfBulbsPerPot { get; set; }
        public string? PlantSpacingCm { get; set; }
        public string? PotSizeCm { get; set; }
        public string? CultivationTimeFromYoungPlant { get; set; }
        public string? CultivationTemperatureC { get; set; }
        public string? NaturalFloweringMonth { get; set; }
        public bool? FlowersInFirstYear { get; set; }
        public bool? GrowthInhibitorsUsed { get; set; }
        public string? PlantingDensity { get; set; }
        

        // ...ďalšie polia podľa nepreškrtnutých názvov z vašej tabuľky...
    }
}
