using System;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace SubtitleOverlay.Services
{
    public class WindowsSpeechRecognitionService
    {
        private SpeechRecognitionEngine? _recognizer;
        private bool _isListening = false;
        private readonly object _lockObject = new object();

        public event EventHandler<string>? SpeechRecognized;
        public event EventHandler<string>? RecognitionError;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        // Create speech recognition engine
                        _recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));
                        
                        // Set up recognition options
                        _recognizer.SetInputToDefaultAudioDevice();
                        
                        // Create grammar for dictation (free-form speech)
                        var dictationGrammar = new DictationGrammar();
                        dictationGrammar.Name = "Dictation Grammar";
                        _recognizer.LoadGrammar(dictationGrammar);
                        
                        // Set up event handlers
                        _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                        _recognizer.SpeechRecognitionRejected += Recognizer_SpeechRecognitionRejected;
                        _recognizer.RecognizeCompleted += Recognizer_RecognizeCompleted;
                        
                        return true;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows Speech Recognition initialization failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StartRecognitionAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        if (_recognizer != null && !_isListening)
                        {
                            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                            _isListening = true;
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Start recognition failed: {ex.Message}");
                return false;
            }
        }

        public async Task StopRecognitionAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        if (_recognizer != null && _isListening)
                        {
                            _recognizer.RecognizeAsyncStop();
                            _isListening = false;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Stop recognition failed: {ex.Message}");
            }
        }

        public async Task<bool> ProcessAudioDataAsync(byte[] audioData)
        {
            try
            {
                // Note: Windows Speech Recognition API doesn't directly accept byte arrays
                // It works with audio devices. For real-time processing of captured audio,
                // we would need to either:
                // 1. Use a virtual audio device
                // 2. Use a different speech recognition library that accepts audio data
                // 3. Save audio to file and process it
                
                // For now, we'll simulate processing the audio data
                // In a real implementation, you would:
                // - Convert audio data to the correct format
                // - Send it to a speech recognition service that accepts audio data
                
                await Task.Delay(100); // Simulate processing time
                
                // Return true to indicate we're ready to process more audio
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio processing failed: {ex.Message}");
                return false;
            }
        }

        private void Recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && e.Result.Confidence > 0.3f)
            {
                var recognizedText = e.Result.Text;
                SpeechRecognized?.Invoke(this, recognizedText);
            }
        }

        private void Recognizer_SpeechRecognitionRejected(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (e.Result != null)
            {
                RecognitionError?.Invoke(this, $"Recognition rejected: {e.Result.Text} (Confidence: {e.Result.Confidence:F2})");
            }
        }

        private void Recognizer_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RecognitionError?.Invoke(this, $"Recognition error: {e.Error.Message}");
            }
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (_recognizer != null)
                {
                    _recognizer.SpeechRecognized -= Recognizer_SpeechRecognized;
                    _recognizer.SpeechRecognitionRejected -= Recognizer_SpeechRecognitionRejected;
                    _recognizer.RecognizeCompleted -= Recognizer_RecognizeCompleted;
                    
                    if (_isListening)
                    {
                        _recognizer.RecognizeAsyncStop();
                    }
                    
                    _recognizer.Dispose();
                    _recognizer = null;
                }
            }
        }

        public bool IsListening => _isListening;
    }
}
