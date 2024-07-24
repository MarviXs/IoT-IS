using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobStatusEnum
{
    JOB_FREE,
    JOB_IDLE,
    JOB_PENDING,
    JOB_PROCESSING,
    JOB_DONE,
    JOB_ERR,
    JOB_PAUSED,
    JOB_CANCELED,
    JOB_STATUS_MAX,
}
