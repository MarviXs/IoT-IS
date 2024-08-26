using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Data.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
    public virtual DateTimeOffset RegistrationDate { get; set; }
    public ICollection<Device> Devices { get; } = [];
    public ICollection<DeviceTemplate> DeviceTemplates { get; } = [];
    public ICollection<CollectionShare> CollectionShares { get; } = [];
}
