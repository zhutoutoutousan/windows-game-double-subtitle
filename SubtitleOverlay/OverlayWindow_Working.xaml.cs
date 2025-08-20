using System.Windows;
using System.Windows.Input;

namespace SubtitleOverlay
{
    public partial class OverlayWindow_Working : Window
    {
        public OverlayWindow_Working()
        {
            InitializeComponent();
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
                OriginalTextBlock.Text = originalText;
                TranslatedTextBlock.Text = translatedText;
            });
        }

        public void UpdateOriginalText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                OriginalTextBlock.Text = text;
            });
        }

        public void UpdateTranslatedText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                TranslatedTextBlock.Text = text;
            });
        }
    }
}
