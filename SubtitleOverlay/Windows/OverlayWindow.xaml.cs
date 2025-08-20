using SubtitleOverlay.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System;

namespace SubtitleOverlay.Windows
{
    public partial class OverlayWindow : Window
    {
        private readonly SubtitleSettings _settings;
        private bool _isDragging = false;
        private Point _dragStart;

        public OverlayWindow(SubtitleSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            ApplySettings();
        }

        private void ApplySettings()
        {
            // Apply font settings
            var fontFamily = new System.Windows.Media.FontFamily(_settings.FontFamily);
            var fontSize = _settings.FontSize;
            var fontColor = (System.Windows.Media.Brush?)new System.Windows.Media.BrushConverter().ConvertFromString(_settings.FontColor) ?? System.Windows.Media.Brushes.White;
            var backgroundColor = (System.Windows.Media.Brush?)new System.Windows.Media.BrushConverter().ConvertFromString(_settings.BackgroundColor) ?? System.Windows.Media.Brushes.Black;

            OriginalTextBlock.FontFamily = fontFamily;
            OriginalTextBlock.FontSize = fontSize;
            OriginalTextBlock.Foreground = fontColor;

            TranslatedTextBlock.FontFamily = fontFamily;
            TranslatedTextBlock.FontSize = fontSize;
            TranslatedTextBlock.Foreground = fontColor;

            // Apply background settings
            var border = (Border)Content;
            border.Background = backgroundColor;
            border.Opacity = _settings.BackgroundOpacity;

            // Set window size based on max width
            Width = _settings.MaxWidth + 50; // Add padding
        }

        public void UpdateSubtitle(string originalText, string? translatedText = null)
        {
            if (!string.IsNullOrWhiteSpace(originalText))
            {
                OriginalTextBlock.Text = originalText;
                OriginalTextBlock.Visibility = _settings.ShowOriginalText ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                OriginalTextBlock.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrWhiteSpace(translatedText))
            {
                TranslatedTextBlock.Text = translatedText;
                TranslatedTextBlock.Visibility = _settings.ShowTranslatedText ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                TranslatedTextBlock.Visibility = Visibility.Collapsed;
            }

            // Auto-hide after a delay if no text
            if (string.IsNullOrWhiteSpace(originalText) && string.IsNullOrWhiteSpace(translatedText))
            {
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, e) =>
                {
                    Hide();
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStart = e.GetPosition(this);
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);
                var offset = currentPosition - _dragStart;
                Left += offset.X;
                Top += offset.Y;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }
    }
}
