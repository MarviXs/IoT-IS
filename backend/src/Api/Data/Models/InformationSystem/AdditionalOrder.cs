namespace Fei.Is.Api.Data.Models.InformationSystem;

public class AdditionalOrder
{
    public int Id { get; set; }
    public string Company { get; set; }
    public DateTime Year { get; set; }
    public int Amount { get; set; }
    public string Type { get; set; }

    public List<Product> Products { get; set; } = new List<Product>();
}
