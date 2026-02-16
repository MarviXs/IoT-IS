using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class SystemNodeSetting : BaseModel
{
    public SystemNodeType NodeType { get; set; } = SystemNodeType.Hub;
    public string? HubUrl { get; set; }
    public string? HubToken { get; set; }
}
