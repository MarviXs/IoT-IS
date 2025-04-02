using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class Device : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty;
    public ApplicationUser? Owner { get; set; } = null!;

    public Guid? DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }

    public required string Name { get; set; }
    public string? Mac { get; set; }
    public required string AccessToken { get; set; }
    public DeviceConnectionProtocol Protocol { get; set; } = DeviceConnectionProtocol.HTTP;

    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<CollectionItem> CollectionItems { get; set; } = [];
    public ICollection<DeviceShare> SharedWithUsers { get; set; } = [];
}
