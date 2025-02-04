using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationSeverity
{
    Info,
    Warning,
    Serious,
    Critical,
}
