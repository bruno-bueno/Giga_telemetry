using Giga_telemetry;
using Giga_telemetry.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelemetryProviderService, TelemetryProviderService>();
builder.Services.AddTransient<IScreenshotService, ScreenshotService>();

builder.Services.AddHttpClient<ITelemetryApiService, TelemetryApiService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["GigaTelemetry:ApiUrl"] ?? "https://telemetry.giganet.dev.br/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
// Auto-Setup Startup (Only on Windows)
if (OperatingSystem.IsWindows())
{
    try
    {
        string appName = "GigaTelemetry";
        // Get the full path to the executable (works for single-file published apps too)
        string appPath = Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
        
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
        if (key != null)
        {
            var existingValue = key.GetValue(appName) as string;
            // Only write if not already set or if path changed
            if (existingValue != appPath)
            {
                key.SetValue(appName, appPath);
            }
        }
    }
    catch (Exception) 
    {
        // Silently fail if unable to set startup (e.g. permissions)
    }
}

host.Run();
