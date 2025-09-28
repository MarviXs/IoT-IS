namespace Fei.Is.Api.Data.Models;

public class DataPoint
{
    public required Guid DeviceId { get; set; }
    public required string SensorTag { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public double? Value { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? GridX { get; set; }
    public int? GridY { get; set; }
}
