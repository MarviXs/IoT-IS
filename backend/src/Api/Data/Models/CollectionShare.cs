using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

// TODO: Implement collection sharing
public class CollectionShare : BaseModel
{
    public Guid CollectionId { get; set; } = Guid.Empty!;
    public DeviceCollection? Collection { get; set; } = null!;
    public Guid UserId { get; set; } = Guid.Empty!;
    public ApplicationUser? User { get; set; } = null!;
    public CollectionPermission Permission { get; set; }
}
