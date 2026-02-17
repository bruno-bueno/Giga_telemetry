using System.Text.Json.Serialization;

namespace Giga_telemetry.Models;

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("uptime")]
    public double Uptime { get; set; }

    [JsonPropertyName("mongo")]
    public string MongoStatus { get; set; } = string.Empty;
}

public class ApiResponse
{
    [JsonPropertyName("inserted")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Inserted { get; set; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }
    
    // For error responses, if needed, though usually standard HTTP errors cover it.
}
