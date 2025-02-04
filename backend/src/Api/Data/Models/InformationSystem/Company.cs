namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Company : BaseModel
{
    public required string Title { get; set; }
    public required string Ic { get; set; }
    public string? Dic { get; set; }
    public string? Street { get; set; }
    public string? Psc { get; set; }
    public string? City { get; set; }
}
