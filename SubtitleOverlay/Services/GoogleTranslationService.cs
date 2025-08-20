using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SubtitleOverlay.Models;
using System.Collections.Concurrent;
using System.Text;
using System.Net.Http;

namespace SubtitleOverlay.Services
{
    public class GoogleTranslationService : ITranslationService
    {
        private readonly ILogger<GoogleTranslationService> _logger;
        private readonly TranslationSettings _settings;
        private readonly ITextCleaningService _textCleaningService;
        private readonly HttpClient _httpClient;
        private readonly ConcurrentDictionary<string, CachedTranslation> _cache;
        private readonly Timer? _cacheCleanupTimer;

        public bool IsAvailable => !string.IsNullOrEmpty(_settings.ApiKey);

        public GoogleTranslationService(ILogger<GoogleTranslationService> logger, IOptions<TranslationSettings> settings, HttpClient httpClient, ITextCleaningService textCleaningService)
        {
            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _textCleaningService = textCleaningService;
            _cache = new ConcurrentDictionary<string, CachedTranslation>();

            if (_settings.CacheEnabled)
            {
                _cacheCleanupTimer = new Timer(CleanupCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            }
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage, string? sourceLanguage = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (!IsAvailable)
            {
                _logger.LogWarning("Translation service not available - API key not configured");
                return text;
            }

            var cacheKey = $"{text}_{sourceLanguage}_{targetLanguage}";
            
            if (_settings.CacheEnabled && _cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            {
                _logger.LogDebug("Translation cache hit for: {Text}", text);
                return cached.TranslatedText;
            }

            try
            {
                var requestData = new
                {
                    q = text,
                    target = targetLanguage,
                    source = sourceLanguage ?? "auto",
                    format = "text"
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{_settings.ApiEndpoint}?key={_settings.ApiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var translationResponse = JsonConvert.DeserializeObject<GoogleTranslationResponse>(responseContent);

                    if (translationResponse?.Data?.Translations?.Any() == true)
                    {
                        var translatedText = translationResponse.Data.Translations[0].TranslatedText;
                        
                        // Improve translation quality using language models
                        var improvedText = await _textCleaningService.ImproveTranslationAsync(translatedText, targetLanguage);
                        
                        if (_settings.CacheEnabled)
                        {
                            var cachedTranslation = new CachedTranslation
                            {
                                TranslatedText = improvedText,
                                CachedAt = DateTime.UtcNow,
                                ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.CacheExpirationMinutes)
                            };
                            _cache.AddOrUpdate(cacheKey, cachedTranslation, (key, old) => cachedTranslation);
                        }

                        _logger.LogDebug("Translated: {Original} -> {Translated} -> {Improved}", text, translatedText, improvedText);
                        return improvedText;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Translation API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate text: {Text}", text);
            }

            return text; // Return original text if translation fails
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !IsAvailable)
                return "en";

            try
            {
                var requestData = new
                {
                    q = text
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://translation.googleapis.com/language/translate/v2/detect?key={_settings.ApiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var detectionResponse = JsonConvert.DeserializeObject<GoogleDetectionResponse>(responseContent);

                    if (detectionResponse?.Data?.Detections?.Any() == true)
                    {
                        return detectionResponse.Data.Detections[0][0].Language;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to detect language for text: {Text}", text);
            }

            return "en"; // Default to English
        }

        private void CleanupCache(object? state)
        {
            var expiredKeys = _cache.Where(kvp => kvp.Value.IsExpired).Select(kvp => kvp.Key).ToList();
            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }
            _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
        }

        private class CachedTranslation
        {
            public string TranslatedText { get; set; } = "";
            public DateTime CachedAt { get; set; }
            public DateTime ExpiresAt { get; set; }
            public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        }

        private class GoogleTranslationResponse
        {
            public TranslationData? Data { get; set; }
        }

        private class TranslationData
        {
            public Translation[]? Translations { get; set; }
        }

        private class Translation
        {
            public string TranslatedText { get; set; } = "";
        }

        private class GoogleDetectionResponse
        {
            public DetectionData? Data { get; set; }
        }

        private class DetectionData
        {
            public Detection[][]? Detections { get; set; }
        }

        private class Detection
        {
            public string Language { get; set; } = "";
        }
    }
}
