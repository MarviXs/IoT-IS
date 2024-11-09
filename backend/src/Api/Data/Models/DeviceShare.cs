using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class DeviceShare : BaseModel
{
    public Guid SharingUserId { get; set; } = Guid.Empty!;
    public ApplicationUser? SharingUser { get; set; } = null!;

    public Guid SharedToUserId { get; set; } = Guid.Empty!;
    public ApplicationUser? SharedToUser { get; set; } = null!;

    public Guid DeviceId { get; set; } = Guid.Empty!;

    public Device? Device { get; set; } = null!;

    public DeviceSharePermission Permission { get; set; }
}
