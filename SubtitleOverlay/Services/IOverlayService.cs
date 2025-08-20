namespace SubtitleOverlay.Services
{
    public interface IOverlayService
    {
        void Show();
        void Hide();
        void UpdateSubtitle(string originalText, string? translatedText = null);
        void SetPosition(double x, double y);
        void SetOpacity(double opacity);
        bool IsVisible { get; }
    }
}
