using System.Text.Json.Serialization;

namespace Giga_telemetry.Models;

public class LogPayload
{
    [JsonPropertyName("machine_id")]
    public string MachineId { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = "info"; // "info" | "warn" | "error" | "fatal"

    [JsonPropertyName("source")]
    public string Source { get; set; } = "agent";

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Context { get; set; }
}
