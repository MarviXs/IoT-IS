using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobStatusEnum
{
    QUEUED,
    IN_PROGRESS,
    PAUSED,
    SUCCEEDED,
    FAILED,
    TIMED_OUT,
    CANCELED,
}
