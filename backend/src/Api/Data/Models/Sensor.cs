namespace Fei.Is.Api.Data.Models;

public class Sensor : BaseModel
{
    public required Guid DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }
    public required string Name { get; set; }
    public required string Tag { get; set; }
    public string? Unit { get; set; }
    public int? AccuracyDecimals { get; set; }
    public int Order { get; set; } = 0;
    public string? Group { get; set; }
}
