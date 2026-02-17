namespace Fei.Is.Api.Data.Models;

public class EdgeNode : BaseModel
{
    public required string Name { get; set; }
    public required string Token { get; set; }
    public int UpdateRateSeconds { get; set; } = 5;
    public ICollection<Device> SyncedDevices { get; set; } = [];
}
