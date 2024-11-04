namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Summary
{
    public int Id { get; set; }
    public string Place { get; set; }
    public Guid ProductNumber { get; set; }
    public int Amount { get; set; }

    public Product Product { get; set; }
}
