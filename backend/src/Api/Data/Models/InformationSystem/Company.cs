namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Company
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Title2 { get; set; }
    public string Ic { get; set; }
    public string Dic { get; set; }
    public string Ulice { get; set; }
    public string Psc { get; set; }
    public string City { get; set; }
    
    public List<DeliveryNote> DeliveryNotes { get; set; } = new List<DeliveryNote>();
    public List<Invoice> Invoices { get; set; } = new List<Invoice>();
}
