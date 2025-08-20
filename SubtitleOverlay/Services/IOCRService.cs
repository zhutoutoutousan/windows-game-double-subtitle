using System.Drawing;
using SubtitleOverlay.Models;

namespace SubtitleOverlay.Services
{
    public interface IOCRService
    {
        event EventHandler<string>? TextRecognized;
        event EventHandler<Exception>? RecognitionError;

        bool IsAvailable { get; }
        bool IsCapturing { get; }
        
        Task StartCaptureAsync(Rectangle captureArea);
        Task StopCaptureAsync();
        Task<string> RecognizeTextAsync(Rectangle captureArea, OCRParameters? parameters = null);
        Task<Rectangle> SelectCaptureAreaAsync();
        void UpdateParameters(OCRParameters parameters);
    }
}
