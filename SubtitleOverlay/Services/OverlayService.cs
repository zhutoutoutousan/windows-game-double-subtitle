using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitleOverlay.Models;
using SubtitleOverlay.Windows;
using System.Windows;
using System.Windows.Threading;

namespace SubtitleOverlay.Services
{
    public class OverlayService : IOverlayService
    {
        private readonly ILogger<OverlayService> _logger;
        private readonly OverlaySettings _settings;
        private readonly SubtitleSettings _subtitleSettings;
        private OverlayWindow? _overlayWindow;
        private readonly Dispatcher _dispatcher;

        public bool IsVisible => _overlayWindow?.IsVisible == true;

        public OverlayService(ILogger<OverlayService> logger, IOptions<OverlaySettings> overlaySettings, IOptions<SubtitleSettings> subtitleSettings)
        {
            _logger = logger;
            _settings = overlaySettings.Value;
            _subtitleSettings = subtitleSettings.Value;
            _dispatcher = Application.Current.Dispatcher;
        }

        public void Show()
        {
            _dispatcher.Invoke(() =>
            {
                if (_overlayWindow == null)
                {
                    _overlayWindow = new OverlayWindow(_subtitleSettings);
                    _overlayWindow.Left = _settings.StartPosition.X;
                    _overlayWindow.Top = _settings.StartPosition.Y;
                    _overlayWindow.Topmost = _settings.AlwaysOnTop;
                }

                if (!_overlayWindow.IsVisible)
                {
                    _overlayWindow.Show();
                    _logger.LogInformation("Overlay window shown");
                }
            });
        }

        public void Hide()
        {
            _dispatcher.Invoke(() =>
            {
                _overlayWindow?.Hide();
                _logger.LogInformation("Overlay window hidden");
            });
        }

        public void UpdateSubtitle(string originalText, string? translatedText = null)
        {
            _dispatcher.Invoke(() =>
            {
                if (_overlayWindow != null)
                {
                    _overlayWindow.UpdateSubtitle(originalText, translatedText);
                    _logger.LogDebug("Subtitle updated: {Original} -> {Translated}", originalText, translatedText ?? "N/A");
                }
            });
        }

        public void SetPosition(double x, double y)
        {
            _dispatcher.Invoke(() =>
            {
                if (_overlayWindow != null)
                {
                    _overlayWindow.Left = x;
                    _overlayWindow.Top = y;
                    _logger.LogDebug("Overlay position updated to ({X}, {Y})", x, y);
                }
            });
        }

        public void SetOpacity(double opacity)
        {
            _dispatcher.Invoke(() =>
            {
                if (_overlayWindow != null)
                {
                    _overlayWindow.Opacity = opacity;
                    _logger.LogDebug("Overlay opacity updated to {Opacity}", opacity);
                }
            });
        }
    }
}
