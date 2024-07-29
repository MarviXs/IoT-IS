namespace Fei.Is.Api.Data.Models;

public class JobCommand : BaseModel
{
    public required Guid JobId { get; set; }
    public Job? Job { get; set; }

    public required int Order { get; set; }

    public required string DisplayName { get; set; }
    public required string Name { get; set; }

    public List<double> Params { get; set; } = [];
}
