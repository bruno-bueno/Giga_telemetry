using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Giga_telemetry.Services;

public interface IScreenshotService
{
    byte[] CaptureScreen();
}

public class ScreenshotService : IScreenshotService
{
    // P/Invoke for Screen Size
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    public byte[] CaptureScreen()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Screenshot service is only supported on Windows.");
        }

        try
        {
            // Capture all screens (Virtual Screen)
            int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
            int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
            int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

            using var bitmap = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bitmap))
            {
                // Capture from the virtual screen coordinates
                g.CopyFromScreen(left, top, 0, 0, new Size(width, height));
            }

            // Efficient conversion to SkiaSharp
            // We lock the bits of the Systems.Drawing.Bitmap and create an SKBitmap from it
            // This avoids an intermediate MemoryStream copy (BMP) which saves memory.
            var data = bitmap.LockBits(
                new Rectangle(0, 0, width, height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            try
            {
                // Create SKImage info matching the bitmap
                var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                
                // Create SKBitmap/SKImage pointing to the locked memory
                using var skBitmap = new SKBitmap();
                skBitmap.InstallPixels(info, data.Scan0, data.Stride); // InstallPixels wraps the memory without copying
                
                // Encode to JPEG 60%
                using var dataEncoded = skBitmap.Encode(SKEncodedImageFormat.Jpeg, 60);
                return dataEncoded.ToArray();
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
        catch (Exception ex)
        {
            // Log error in real app
            Console.WriteLine($"Screenshot failed: {ex.Message}");
            return Array.Empty<byte>();
        }
    }
}
