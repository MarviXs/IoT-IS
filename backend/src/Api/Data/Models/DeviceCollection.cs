namespace Fei.Is.Api.Data.Models;

public class DeviceCollection : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty!;
    public ApplicationUser? Owner { get; set; } = null!;
    public required string Name { get; set; }
    public bool IsRoot { get; set; } = true;
    public ICollection<CollectionItem> Items { get; set; } = [];
    public ICollection<CollectionItem> SubItems { get; set; } = [];
}
