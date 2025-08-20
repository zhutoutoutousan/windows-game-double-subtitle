using System.Threading.Tasks;

namespace SubtitleOverlay.Services
{
    public class SimpleTranslationService : ITranslationService
    {
        public bool IsAvailable => true;

        public async Task<string> TranslateAsync(string text, string targetLanguage, string? sourceLanguage = null)
        {
            // For now, just return the original text
            // This can be extended to use Google Translate API or other services
            return await Task.FromResult(text);
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            // For now, just return English as default
            // This can be extended to use language detection services
            return await Task.FromResult("en");
        }
    }
}
