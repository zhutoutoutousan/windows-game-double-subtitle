namespace SubtitleOverlay.Services
{
    public interface ITranslationService
    {
        Task<string> TranslateAsync(string text, string targetLanguage, string? sourceLanguage = null);
        Task<string> DetectLanguageAsync(string text);
        bool IsAvailable { get; }
    }
}
