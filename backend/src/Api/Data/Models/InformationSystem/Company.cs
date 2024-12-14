namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Company : BaseModel
{
    public required string Title { get; set; }
    public string? Title2 { get; set; }
    public required string Ic { get; set; }
    public required string Dic { get; set; }
    public required string Ulice { get; set; }
    public required string Psc { get; set; }
    public required string City { get; set; }
}
