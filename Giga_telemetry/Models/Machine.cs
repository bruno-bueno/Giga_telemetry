using System.Text.Json.Serialization;

namespace Giga_telemetry.Models;

public class Machine
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("machine_id")]
    public string MachineId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "active" | "disabled"

    [JsonPropertyName("last_seen_at")]
    public DateTime LastSeenAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
