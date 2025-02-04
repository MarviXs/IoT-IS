namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Summary : BaseModel
{
    public string Place { get; set; }
    public int Amount { get; set; }
    public Product Product { get; set; }
}
