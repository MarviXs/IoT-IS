namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    
    public List<Product> Products { get; set; } = new List<Product>();
}