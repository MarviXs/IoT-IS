namespace Fei.Is.Api.Data.Models;

public class Job : BaseModel
{
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public List<JobCommand> Commands { get; set; } = [];
    public JobStatus? Status { get; set; }
    public required string Name { get; set; }
    public int NoOfCmds { get; set; } = 0;
    public int NoOfReps { get; set; } = 0;
    public bool ToCancel { get; set; } = false;
    public bool Paused { get; set; } = false;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
