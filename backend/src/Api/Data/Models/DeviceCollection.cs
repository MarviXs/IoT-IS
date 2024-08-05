namespace Fei.Is.Api.Data.Models;

public class DeviceCollection : BaseModel
{
    public string OwnerId { get; set; } = null!;
    public User? Owner { get; set; } = null!;
    public required string Name { get; set; }
    public bool IsRoot { get; set; } = true;
    public ICollection<CollectionItem> Items { get; set; } = [];
    public ICollection<CollectionItem> SubItems { get; set; } = [];
}
