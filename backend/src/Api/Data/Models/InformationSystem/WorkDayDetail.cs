namespace Fei.Is.Api.Data.Models.InformationSystem;

public class WorkDayDetail
{
    public int Id { get; set; }
    
    // Foreign Key: WorkReport reference
    public int ReportId { get; set; }
    public WorkReport WorkReport { get; set; }

    public DateTime WorkDate { get; set; }
    public int TaskNumber { get; set; }
    public string WorkLocation { get; set; }
    public string WorkType { get; set; }
    public int WorkersCount { get; set; }
    public int WorkHours { get; set; }
    
    public decimal RateA { get; set; } // Hourly rate A (e.g., 285 Kč/hod)
    public decimal RateB { get; set; } // Hourly rate B (e.g., 355 Kč/hod)
    public decimal TotalA { get; set; } // Total price for work type A
    public decimal TotalB { get; set; } // Total price for work type B

    public string EquipmentType { get; set; }
    public int EquipmentQuantity { get; set; }
    public decimal EquipmentPrice { get; set; }
    public decimal EquipmentTotal { get; set; }

    public string MaterialType { get; set; }
    public int MaterialQuantity { get; set; }
    public decimal MaterialPrice { get; set; }
    public decimal MaterialTotal { get; set; }

    public decimal TotalPrice { get; set; } // Total price for work done on the day
}
