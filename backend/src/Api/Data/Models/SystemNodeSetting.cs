using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class SystemNodeSetting : BaseModel
{
    public SystemNodeType NodeType { get; set; } = SystemNodeType.Hub;
    public string? HubUrl { get; set; }
    public string? HubToken { get; set; }
    public int SyncIntervalSeconds { get; set; } = 5;
    public EdgeDataPointSyncMode DataPointSyncMode { get; set; } = EdgeDataPointSyncMode.OnlyNew;
    public long? BackfillCutoffUnixMs { get; set; }
    public long? BackfillCursorTimestampUnixMs { get; set; }
    public int BackfillCursorOffset { get; set; } = 0;
    public bool BackfillCompleted { get; set; } = false;
}
