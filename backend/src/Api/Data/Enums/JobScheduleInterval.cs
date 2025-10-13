using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobScheduleIntervalEnum
{
    Minute,
    Hour,
    Day,
    Week
}
