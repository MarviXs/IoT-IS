using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Data.Models;

public class User : IdentityUser
{
    public ICollection<Device> Devices { get; } = [];
    public ICollection<DeviceTemplate> DeviceTemplates { get; } = [];
}
