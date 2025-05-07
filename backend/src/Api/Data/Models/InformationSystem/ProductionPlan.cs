namespace Fei.Is.Api.Data.Models.InformationSystem;

public class ProductionPlan : BaseModel
{
    public int Year { get; set; }
    public int DeliveryWeek { get; set; }
    public int OrderedQuantity { get; set; }
    public int StrQuantity { get; set; }
    public int CbQuantity { get; set; }
    public int VolyneQuantity { get; set; }
    public int TabQuantity { get; set; }
    public int StoreQuantity { get; set; }
    public int Pot9 { get; set; }
    public int Pot10 { get; set; }
    public int Pot12 { get; set; }
    public int Pot14 { get; set; }
    public int Pot17 { get; set; }
    public int Pot21 { get; set; }
    public int Pack10 { get; set; }
    public int Pack6 { get; set; }
    public int Z { get; set; }
    public int M6 { get; set; }
    public int M10 { get; set; }
    public int S84 { get; set; }
    public int TotalQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public Product Product { get; set; }
}
