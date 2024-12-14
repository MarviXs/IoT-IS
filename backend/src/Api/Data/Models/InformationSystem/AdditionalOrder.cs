namespace Fei.Is.Api.Data.Models.InformationSystem;

public class AdditionalOrder : BaseModel
{
    public int Company { get; set; }
    public DateTime Year { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; }

    public Product Product { get; set; }
}
