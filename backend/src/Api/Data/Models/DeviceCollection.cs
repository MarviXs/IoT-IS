namespace Fei.Is.Api.Data.Models;

public class DeviceCollection : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty!;
    public ApplicationUser? Owner { get; set; } = null!;
    public required string Name { get; set; }
    public bool IsRoot { get; set; } = true;

    public Guid RootCollectionId { get; set; } = Guid.Empty!;
    public DeviceCollection? RootCollection { get; set; } = null!;
    public ICollection<CollectionItem> AllCollectionsFlat { get; set; } = [];

    public ICollection<CollectionItem> ChildItems { get; set; } = [];
    public ICollection<CollectionItem> ParentItems { get; set; } = [];
    public ICollection<CollectionShare> CollectionShares { get; set; } = [];
}
