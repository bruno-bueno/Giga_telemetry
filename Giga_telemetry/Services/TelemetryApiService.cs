using System.Net.Http.Json;
using System.Net.Http.Headers;
using Giga_telemetry.Models;

namespace Giga_telemetry.Services;

public interface ITelemetryApiService
{
    Task<bool> SendTelemetryAsync(TelemetryPayload payload);
    Task<bool> SendScreenshotAsync(byte[] imageBytes, string machineId);
}

public class TelemetryApiService : ITelemetryApiService
{
    private readonly HttpClient _httpClient;
    private const int MaxRetries = 3;

    public TelemetryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Base address should be configured in Program.cs, but default here if needed
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri("http://localhost:3000");
        }
    }

    public async Task<bool> SendTelemetryAsync(TelemetryPayload payload)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/telemetry");
            request.Headers.Add("x-machine-id", payload.MachineId);
            request.Content = JsonContent.Create(payload);
            return await _httpClient.SendAsync(request);
        });
    }

    public async Task<bool> SendScreenshotAsync(byte[] imageBytes, string machineId)
    {
        if (imageBytes == null || imageBytes.Length == 0) return false;

        return await ExecuteWithRetryAsync(async () =>
        {
            var content = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "file", "screenshot.jpg"); // Content is disposed by request/response? No, explicit dispose needed usually.
            // Using logic inside here is tricky with Func. 
            // We'll dispose manually or rely on GC if not excessive. 
            // Better: Create content, send, then dispose.
            
            var request = new HttpRequestMessage(HttpMethod.Post, "/artifacts/screenshot");
            request.Headers.Add("x-machine-id", machineId);
            request.Headers.Add("x-artifact-type", "screenshot"); 
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            // Dispose content after send (MultipartFormDataContent must be disposed)
            content.Dispose();
            
            return response;
        });
    }

    private async Task<bool> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> action)
    {
        for (int i = 0; i < MaxRetries; i++)
        {
            try
            {
                using var response = await action();
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                // Don't retry client errors (4xx)
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return false;
                }
                
                // Retry specific server errors 5xx
            }
            catch (Exception)
            {
                // Retry on exception (network failure)
            }

            if (i < MaxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
            }
        }
        return false;
    }
}
