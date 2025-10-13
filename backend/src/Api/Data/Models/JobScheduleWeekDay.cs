using Fei.Is.Api.Data.Enums;

namespace Fei.Is.Api.Data.Models;

public class JobScheduleWeekDay : BaseModel
{
    public Guid JobScheduleId { get; set; }
    public JobSchedule? JobSchedule { get; set; }
    public JobScheduleWeekDayEnum Day { get; set; }
}
