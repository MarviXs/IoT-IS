namespace Fei.Is.Api.Data.Models;

public class Device : BaseModel
{
    public string OwnerId { get; set; } = null!;
    public User? Owner { get; set; } = null!;

    public Guid? DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }

    public required string Name { get; set; }
    public string? Mac { get; set; }
    public required string AccessToken { get; set; }
    
    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<CollectionItem> CollectionItems { get; set; } = [];
}
