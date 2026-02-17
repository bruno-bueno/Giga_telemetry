using System.Diagnostics;
using System.Net.NetworkInformation;
using Giga_telemetry.Models;

namespace Giga_telemetry.Services;

public interface ITelemetryProviderService
{
    TelemetryPayload GetTelemetry();
}

public class TelemetryProviderService : ITelemetryProviderService
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memAvailableCounter;
    private readonly string _machineId;
    
    // Cache total memory to avoid recalculation/calls
    private readonly long _totalMemoryBytes;

    public TelemetryProviderService()
    {
        // Only works on Windows
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("This service is designed for Windows.");
        }

        _machineId = GetMachineId();
        
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memAvailableCounter = new PerformanceCounter("Memory", "Available MBytes");
        
        // Initial call usually returns 0, so we call it once here
        _cpuCounter.NextValue();

        // Get total physical memory. GC.GetGCMemoryInfo().TotalAvailableMemoryBytes 
        // usually returns the total physical memory available to the runtime.
        _totalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    }

    public TelemetryPayload GetTelemetry()
    {
        var cpuUsage = _cpuCounter.NextValue();
        var availableMemMb = _memAvailableCounter.NextValue();
        var totalMemMb = _totalMemoryBytes / 1024 / 1024;
        
        // Memory Used = Total - Available
        // Note: Available from PerfCounter is what's free. 
        // Used = Total - Available.
        var usedMemMb = totalMemMb - availableMemMb;

        var payload = new TelemetryPayload
        {
            MachineId = _machineId,
            Timestamp = DateTime.UtcNow,
            Cpu = new CpuInfo { Usage = Math.Round(cpuUsage, 2) },
            Memory = new MemoryInfo 
            { 
                Total = totalMemMb, 
                Used = (long)usedMemMb 
            },
            Net = GetNetworkMetrics(),
            Disk = GetDiskMetrics()
        };

        return payload;
    }

    private List<DiskInfo> GetDiskMetrics()
    {
        var disks = new List<DiskInfo>();
        try
        {
            var drive = new DriveInfo("C");
            if (drive.IsReady)
            {
                disks.Add(new DiskInfo
                {
                    Mount = drive.Name, // "C:\"
                    Total = drive.TotalSize / 1024 / 1024, // MB
                    Used = (drive.TotalSize - drive.TotalFreeSpace) / 1024 / 1024
                });
            }
        }
        catch (Exception)
        {
            // Ignore if C drive access fails
        }
        return disks;
    }

    private NetInfo GetNetworkMetrics()
    {
        long totalSent = 0;
        long totalReceived = 0;

        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in interfaces)
        {
            if (ni.OperationalStatus == OperationalStatus.Up && 
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var stats = ni.GetIPv4Statistics();
                totalSent += stats.BytesSent;
                totalReceived += stats.BytesReceived;
            }
        }

        return new NetInfo
        {
            Tx = totalSent,
            Rx = totalReceived
        };
    }

    private string GetMachineId()
    {
        // 1. Try to get Windows MachineGuid from Registry (Very stable)
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                if (key != null)
                {
                    var id = key.GetValue("MachineGuid") as string;
                    if (!string.IsNullOrEmpty(id))
                    {
                        return id;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore registry access errors
            }
        }

        // 2. Fallback: Create/Read a file in CommonApplicationData (ProgramData)
        try
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var path = Path.Combine(folder, "GigaTelemetry", "machine_id.txt");
            
            if (File.Exists(path))
            {
                return File.ReadAllText(path).Trim();
            }
            
            // Create folder if not exists
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Generate new ID and save
            var newId = Guid.NewGuid().ToString();
            File.WriteAllText(path, newId);
            return newId;
        }
        catch (Exception)
        {
            // 3. Last resort: Return a random GUID if filesystem fails (should happen rarely)
            return Guid.NewGuid().ToString();
        }
    }
}
