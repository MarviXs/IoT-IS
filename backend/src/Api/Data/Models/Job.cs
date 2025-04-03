using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class Job : BaseModel
{
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public List<JobCommand> Commands { get; set; } = [];
    public required JobStatusEnum Status { get; set; }
    public required string Name { get; set; }
    public int CurrentStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 1;
    public int CurrentCycle { get; set; } = 1;
    public int TotalCycles { get; set; } = 1;
    public bool Paused { get; set; } = false;
    public bool IsInfinite { get; set; } = false;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
