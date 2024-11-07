using System.Text.Json;
using Fei.Is.Api.Common.Errors;
using FluentResults;

namespace Fei.Is.Api.Features.Scenes.Utils;

public class SceneConditionUtils
{
    public record SceneTriggerInfo(Guid DeviceId, string Tag);

    public static Result<List<SceneTriggerInfo>> ParseCondition(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return Result.Fail(new ValidationError("Condition", "Condition cannot be null or empty."));
        }
        try
        {
            using var document = JsonDocument.Parse(condition);
            var root = document.RootElement;

            var triggers = new List<SceneTriggerInfo>();
            ExtractTriggers(root, triggers);
            return Result.Ok(triggers);
        }
        catch (JsonException ex)
        {
            return Result.Fail(new ValidationError("Invalid JSON", $"Invalid JSON in condition: {ex.Message}"));
        }
    }

    private static void ExtractTriggers(JsonElement element, List<SceneTriggerInfo> triggers)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name == "var" && property.Value.ValueKind == JsonValueKind.String)
                {
                    var varValue = property.Value.GetString();
                    if (TryParseVar(varValue, out var deviceId, out var tag))
                    {
                        triggers.Add(new SceneTriggerInfo(deviceId, tag));
                    }
                }
                else
                {
                    ExtractTriggers(property.Value, triggers);
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractTriggers(item, triggers);
            }
        }
    }

    private static bool TryParseVar(string var, out Guid deviceId, out string tag)
    {
        deviceId = Guid.Empty;
        tag = string.Empty;

        if (!var.StartsWith("device.", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = var.Split('.');
        if (parts.Length < 3)
            return false;

        if (!Guid.TryParse(parts[1], out deviceId))
            return false;

        tag = string.Join('.', parts.Skip(2)); // In case the tag contains dots

        return true;
    }
}
