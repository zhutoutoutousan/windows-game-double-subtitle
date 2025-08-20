using System.Threading.Tasks;

namespace SubtitleOverlay.Services
{
    public interface ITextCleaningService
    {
        Task<string> CleanTextAsync(string rawText, string sourceLanguage = "en");
        Task<string> FixOCRTextAsync(string ocrText, string sourceLanguage = "en");
        Task<string> ImproveTranslationAsync(string text, string targetLanguage = "en");
    }
}
