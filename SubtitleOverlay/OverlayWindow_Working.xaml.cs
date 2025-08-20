using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using SubtitleOverlay.Models;

namespace SubtitleOverlay
{
    public partial class OverlayWindow_Working : Window
    {
        private SubtitleSettings _settings;

        public OverlayWindow_Working(SubtitleSettings? settings = null)
        {
            InitializeComponent();
            _settings = settings ?? new SubtitleSettings
            {
                ShowOriginalText = true,
                ShowTranslatedText = true
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        public void UpdateSubtitles(string originalText, string translatedText = "")
        {
            Dispatcher.Invoke(() =>
            {
                // Update original text block
                if (!string.IsNullOrWhiteSpace(originalText))
                {
                    OriginalTextBlock.Text = originalText;
                    OriginalTextBlock.Visibility = _settings.ShowOriginalText ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    OriginalTextBlock.Visibility = Visibility.Collapsed;
                }

                // Update translated text block
                if (!string.IsNullOrWhiteSpace(translatedText))
                {
                    TranslatedTextBlock.Text = translatedText;
                    TranslatedTextBlock.Visibility = _settings.ShowTranslatedText ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    TranslatedTextBlock.Visibility = Visibility.Collapsed;
                }

                // Auto-scroll to bottom after updating text
                ScrollToBottom();
                
                // Auto-resize window after a short delay to allow text to render
                Dispatcher.BeginInvoke(new Action(() => AutoResizeWindow()), System.Windows.Threading.DispatcherPriority.Loaded);
            });
        }

        public void UpdateOriginalText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    OriginalTextBlock.Text = text;
                    OriginalTextBlock.Visibility = _settings.ShowOriginalText ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    OriginalTextBlock.Visibility = Visibility.Collapsed;
                }

                // Auto-scroll to bottom after updating text
                ScrollToBottom();
                
                // Auto-resize window after a short delay to allow text to render
                Dispatcher.BeginInvoke(new Action(() => AutoResizeWindow()), System.Windows.Threading.DispatcherPriority.Loaded);
            });
        }

        public void UpdateTranslatedText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    TranslatedTextBlock.Text = text;
                    TranslatedTextBlock.Visibility = _settings.ShowTranslatedText ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    TranslatedTextBlock.Visibility = Visibility.Collapsed;
                }

                // Auto-scroll to bottom after updating text
                ScrollToBottom();
                
                // Auto-resize window after a short delay to allow text to render
                Dispatcher.BeginInvoke(new Action(() => AutoResizeWindow()), System.Windows.Threading.DispatcherPriority.Loaded);
            });
        }

        private void ScrollToBottom()
        {
            try
            {
                // Find the ScrollViewer and scroll to bottom
                var scrollViewer = FindScrollViewer();
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ScrollToBottom error: {ex.Message}");
            }
        }

        private ScrollViewer? FindScrollViewer()
        {
            // Find the ScrollViewer in the visual tree
            return FindVisualChild<ScrollViewer>(this);
        }

        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }
            return null;
        }

        public void AutoResizeWindow()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Calculate the required height based on content
                    double requiredHeight = 0;
                    
                    if (OriginalTextBlock.Visibility == Visibility.Visible)
                    {
                        requiredHeight += OriginalTextBlock.ActualHeight;
                    }
                    
                    if (TranslatedTextBlock.Visibility == Visibility.Visible)
                    {
                        requiredHeight += TranslatedTextBlock.ActualHeight;
                    }
                    
                    // Add padding and margins
                    requiredHeight += 60; // Margin + padding + some buffer
                    
                    // Set minimum and maximum height constraints
                    double newHeight = Math.Max(100, Math.Min(600, requiredHeight));
                    
                    // Only resize if the height difference is significant
                    if (Math.Abs(Height - newHeight) > 20)
                    {
                        Height = newHeight;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AutoResizeWindow error: {ex.Message}");
            }
        }

        public void ClearText()
        {
            Dispatcher.Invoke(() =>
            {
                OriginalTextBlock.Text = "";
                TranslatedTextBlock.Text = "";
                
                // Reset scroll position to top
                var scrollViewer = FindScrollViewer();
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToTop();
                }
                
                // Reset window height
                Height = 200;
            });
        }

        public void SetMaxHeight(double maxHeight)
        {
            Dispatcher.Invoke(() =>
            {
                MaxHeight = maxHeight;
            });
        }
    }
}
