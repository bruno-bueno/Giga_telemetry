using Giga_telemetry;
using Giga_telemetry.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelemetryProviderService, TelemetryProviderService>();
builder.Services.AddTransient<IScreenshotService, ScreenshotService>();

builder.Services.AddHttpClient<ITelemetryApiService, TelemetryApiService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["GigaTelemetry:ApiUrl"] ?? "http://localhost:3000";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
