namespace Fei.Is.Api.Data.Models;

public class Experiment : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty;
    public ApplicationUser? Owner { get; set; } = null!;

    public Guid? DeviceId { get; set; }
    public Device? Device { get; set; }

    public string? Note { get; set; }

    public Guid? RecipeToRunId { get; set; }
    public Recipe? RecipeToRun { get; set; }

    public Guid? RanJobId { get; set; }
    public Job? RanJob { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
