namespace Fei.Is.Api.Data.Models;

public class CollectionItem : BaseModel
{
    public Guid CollectionParentId { get; set; }
    public DeviceCollection? CollectionParent { get; set; } = null!;
    
    public Guid? DeviceId { get; set; }
    public Device? Device { get; set; } = null!;

    public Guid? SubCollectionId { get; set; }
    public DeviceCollection? SubCollection { get; set; } = null!;
}
