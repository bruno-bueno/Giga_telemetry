using Giga_telemetry.Services;

namespace Giga_telemetry
{
    public class Worker(
        ILogger<Worker> logger,
        ITelemetryProviderService telemetryProvider,
        IScreenshotService screenshotService,
        ITelemetryApiService apiService) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Giga Telemetry Agent started.");

            // Loop principal
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var timestamp = DateTime.UtcNow;
                    logger.LogInformation("Collecting telemetry at: {time}", timestamp);

                    // 1. Coletar e enviar Telemetria
                    var telemetry = telemetryProvider.GetTelemetry();
                    var telemetrySent = await apiService.SendTelemetryAsync(telemetry);

                    if (telemetrySent)
                    {
                        logger.LogInformation("Telemetry sent successfully.");
                    }
                    else
                    {
                        logger.LogWarning("Failed to send telemetry.");
                    }

                    // 1.5. Enviar Log de exemplo
                    var logPayload = new Giga_telemetry.Models.LogPayload
                    {
                        MachineId = telemetry.MachineId,
                        Timestamp = DateTime.UtcNow,
                        Level = "info",
                        Source = "agent",
                        Message = "Ciclo de coleta finalizado",
                        Context = new { memoryUsed = telemetry.Memory.Used }
                    };
                    var logSent = await apiService.SendLogAsync(logPayload);
                    if (logSent)
                    {
                        logger.LogInformation("Log sent successfully.");
                    }
                    else
                    {
                        logger.LogWarning("Failed to send log.");
                    }

                    // 2. Capturar e enviar Screenshot
                    // Apenas um exemplo: enviar screenshot a cada ciclo também
                    logger.LogInformation("Capturing screenshot...");
                    var screenshotBytes = screenshotService.CaptureScreen();
                    
                    if (screenshotBytes.Length > 0)
                    {
                        var screenshotSent = await apiService.SendScreenshotAsync(screenshotBytes, telemetry.MachineId);
                        if (screenshotSent)
                        {
                            logger.LogInformation("Screenshot uploaded successfully ({size} bytes).", screenshotBytes.Length);
                        }
                        else
                        {
                            logger.LogWarning("Failed to upload screenshot.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in worker loop");
                }

                // Aguarda 30 segundos antes do próximo ciclo
                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}
