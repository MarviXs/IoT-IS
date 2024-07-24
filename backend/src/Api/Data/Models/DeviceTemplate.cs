using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class DeviceTemplate : BaseModel
{
    public string OwnerId { get; set; } = null!;
    public User? Owner { get; set; } = null!;
    public required string Name { get; set; }
    public required string ModelId { get; set; }
    public ICollection<Sensor> Sensors { get; set; } = [];
    public ICollection<Command> Commands { get; set; } = [];
    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
}
