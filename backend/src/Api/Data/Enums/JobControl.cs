using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobControl
{
    JOB_PAUSE,
    JOB_RESUME,
    JOB_SKIP_STEP,
    JOB_SKIP_CYCLE,
    JOB_CANCEL
}
