namespace Fei.Is.Api.Data.Models;

public class Experiment : BaseModel
{
    public Guid OwnerId { get; set; } = Guid.Empty;
    public ApplicationUser? Owner { get; set; } = null!;

    public string? Note { get; set; }

    public required Guid? RecipeToRunId { get; set; }
    public Recipe? RecipeToRun { get; set; }

    public Guid? RanJobId { get; set; }
    public Job? RanJob { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
