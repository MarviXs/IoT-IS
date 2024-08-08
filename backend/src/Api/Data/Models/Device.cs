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
    
    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<CollectionItem> CollectionItems { get; set; } = [];
}
