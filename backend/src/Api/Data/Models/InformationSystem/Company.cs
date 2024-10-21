namespace Fei.Is.Api.Data.Models.InformationSystem;

public class Company
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Title2 { get; set; }
    public string IC { get; set; }
    public string DIC { get; set; }
    public string Ulice { get; set; }
    public string PSC { get; set; }
    public string City { get; set; }

    // mozno + lics orders
    public List<DeliveryNote> DeliveryNotes { get; set; } = new List<DeliveryNote>();
    public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    public List<WorkReport> WorkReports { get; set; } = new List<WorkReport>();
}
