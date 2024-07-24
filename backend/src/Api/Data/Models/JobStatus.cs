using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class JobStatus: BaseModel
{
    public required Guid JobId { get; set; }
    public Job? Job { get; set; }
    public required JobStatusEnum RetCode { get; set; }
    public required JobStatusEnum Code { get; set; }
    public int CurrentStep { get; set; } = 0;
    public int TotalSteps { get; set; } = 0;
    public int CurrentCycle { get; set; } = 0;
}
