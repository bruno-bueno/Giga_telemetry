using System.Text.Json.Serialization;

namespace Giga_telemetry.Models;

public class TelemetryPayload
{
    [JsonPropertyName("machine_id")]
    public string MachineId { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("cpu")]
    public CpuInfo Cpu { get; set; } = new();

    [JsonPropertyName("memory")]
    public MemoryInfo Memory { get; set; } = new();

    [JsonPropertyName("disk")]
    public List<DiskInfo> Disk { get; set; } = new();

    [JsonPropertyName("net")]
    public NetInfo Net { get; set; } = new();
}

public class CpuInfo
{
    [JsonPropertyName("usage")]
    public double Usage { get; set; }
}

public class MemoryInfo
{
    [JsonPropertyName("used")]
    public long Used { get; set; }

    [JsonPropertyName("total")]
    public long Total { get; set; }
}

public class DiskInfo
{
    [JsonPropertyName("mount")]
    public string Mount { get; set; } = string.Empty;

    [JsonPropertyName("used")]
    public long Used { get; set; }

    [JsonPropertyName("total")]
    public long Total { get; set; }
}

public class NetInfo
{
    [JsonPropertyName("rx")]
    public long Rx { get; set; }

    [JsonPropertyName("tx")]
    public long Tx { get; set; }
}
