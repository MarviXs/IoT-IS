using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class JobSchedule : BaseModel
{
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public string Name { get; set; } = null!;
    public Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public JobScheduleTypeEnum Type { get; set; }
    public JobScheduleIntervalEnum? Interval { get; set; }
    public int? IntervalValue { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public List<JobScheduleWeekDay> WeekDays { get; set; } = [];
    public int Cycles { get; set; }
    public bool IsActive { get; set; }
}
