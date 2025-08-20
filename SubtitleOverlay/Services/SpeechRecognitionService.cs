using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitleOverlay.Models;
using System.Speech.Recognition;

namespace SubtitleOverlay.Services
{
    public class SpeechRecognitionService : ISpeechRecognitionService, IDisposable
    {
        private readonly ILogger<SpeechRecognitionService> _logger;
        private readonly SpeechRecognitionSettings _settings;
        private SpeechRecognitionEngine? _recognizer;
        private bool _disposed;

        public event EventHandler<string>? SpeechRecognized;
        public event EventHandler<string>? SpeechHypothesis;
        public event EventHandler<Exception>? RecognitionError;

        public bool IsListening => _recognizer?.AudioState == AudioState.Silence;

        public SpeechRecognitionService(ILogger<SpeechRecognitionService> logger, IOptions<SpeechRecognitionSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task StartAsync()
        {
            try
            {
                if (_recognizer != null)
                {
                    await StopAsync();
                }

                _recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
                
                if (_settings.UseDictationMode)
                {
                    _recognizer.LoadGrammar(new DictationGrammar());
                }
                else if (!string.IsNullOrEmpty(_settings.GrammarFile))
                {
                    _recognizer.LoadGrammar(new Grammar(_settings.GrammarFile));
                }
                else
                {
                    // Load default grammar for common phrases
                    var grammar = new GrammarBuilder();
                    grammar.AppendDictation();
                    _recognizer.LoadGrammar(new Grammar(grammar));
                }

                _recognizer.SpeechRecognized += OnSpeechRecognized;
                _recognizer.SpeechHypothesized += OnSpeechHypothesized;
                _recognizer.SpeechRecognitionRejected += OnSpeechRecognitionRejected;
                _recognizer.RecognizeCompleted += OnRecognizeCompleted;

                _recognizer.SetInputToDefaultAudioDevice();
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);

                _logger.LogInformation("Speech recognition started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start speech recognition");
                RecognitionError?.Invoke(this, ex);
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (_recognizer != null)
            {
                _recognizer.RecognizeAsyncStop();
                _recognizer.Dispose();
                _recognizer = null;
                _logger.LogInformation("Speech recognition stopped");
            }
            await Task.CompletedTask;
        }

        public void SetConfidenceThreshold(double threshold)
        {
            // Note: System.Speech doesn't directly support confidence threshold
            // This would need to be implemented in the recognition handlers
        }

        private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= _settings.ConfidenceThreshold)
            {
                var recognizedText = e.Result.Text;
                _logger.LogDebug("Speech recognized: {Text} (confidence: {Confidence})", recognizedText, e.Result.Confidence);
                SpeechRecognized?.Invoke(this, recognizedText);
            }
        }

        private void OnSpeechHypothesized(object? sender, SpeechHypothesizedEventArgs e)
        {
            var hypothesisText = e.Result.Text;
            _logger.LogDebug("Speech hypothesis: {Text}", hypothesisText);
            SpeechHypothesis?.Invoke(this, hypothesisText);
        }

        private void OnSpeechRecognitionRejected(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            _logger.LogDebug("Speech recognition rejected. Alternatives: {Alternatives}", 
                string.Join(", ", e.Result.Alternates.Select(a => $"{a.Text} ({a.Confidence})")));
        }

        private void OnRecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _logger.LogError(e.Error, "Speech recognition error");
                RecognitionError?.Invoke(this, e.Error);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _recognizer?.Dispose();
                _disposed = true;
            }
        }
    }
}
