using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobStatusEnum
{
    JOB_QUEUED,
    JOB_IN_PROGRESS,
    JOB_PAUSED,
    JOB_SUCCEEDED,
    JOB_REJECTED,
    JOB_FAILED,
    JOB_TIMED_OUT,
    JOB_CANCELED,
}
