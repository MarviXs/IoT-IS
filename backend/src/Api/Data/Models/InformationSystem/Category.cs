namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Category : BaseModel
{
    public required string CategoryName { get; set; }

    public List<Product> Products { get; set; } = new List<Product>();
}