using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class DeviceTemplate : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty!;
    public ApplicationUser? Owner { get; set; } = null!;
    public required string Name { get; set; }
    public DeviceType DeviceType { get; set; } = DeviceType.Generic;
    public ICollection<Sensor> Sensors { get; set; } = [];
    public ICollection<Command> Commands { get; set; } = [];
    public ICollection<Recipe> Recipes { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
    public int? GridRowSpan { get; set; }
    public int? GridColumnSpan { get; set; }
}
