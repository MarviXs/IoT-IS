namespace Fei.Is.Api.Data.Models;

public class DeviceFirmware : BaseModel
{
    public Guid DeviceTemplateId { get; set; }
    public DeviceTemplate? DeviceTemplate { get; set; }
    public required string VersionNumber { get; set; }
    public bool IsActive { get; set; }
    public required string OriginalFileName { get; set; }
    public required string StoredFileName { get; set; }
}
