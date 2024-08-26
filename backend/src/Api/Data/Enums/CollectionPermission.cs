using System.Text.Json.Serialization;

namespace Fei.Is.Api.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CollectionPermission
{
    Owner,
    Editor,
    Viewer
}
