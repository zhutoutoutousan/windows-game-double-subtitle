using System;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleOverlay.Services
{
    public class FreeSpeechRecognitionService : IDisposable
    {
        private SpeechRecognitionEngine? _speechEngine;
        private bool _isInitialized = false;
        private bool _isListening = false;

        public event EventHandler<string>? SpeechRecognized;
        public event EventHandler<string>? RecognitionError;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Attempting to initialize Windows Speech Recognition...");
                        
                        // Check if speech recognizers are available
                        var recognizers = SpeechRecognitionEngine.InstalledRecognizers();
                        System.Diagnostics.Debug.WriteLine($"Found {recognizers.Count} installed speech recognizers");
                        
                        if (recognizers.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("No speech recognizers installed on this system");
                            return false;
                        }
                        
                        // Try to use Windows Speech Recognition (free, built into Windows)
                        var culture = new CultureInfo("en-US");
                        var recognizer = recognizers.FirstOrDefault(r => r.Culture.Equals(culture));
                        
                        if (recognizer == null)
                        {
                            // Fall back to any available recognizer
                            recognizer = recognizers.FirstOrDefault();
                            System.Diagnostics.Debug.WriteLine($"en-US recognizer not found, using: {recognizer?.Culture}");
                        }
                        
                        if (recognizer == null)
                        {
                            System.Diagnostics.Debug.WriteLine("No suitable speech recognizer found");
                            return false;
                        }
                        
                        _speechEngine = new SpeechRecognitionEngine(recognizer.Id);
                        System.Diagnostics.Debug.WriteLine($"Created speech engine with recognizer: {recognizer.Description}");
                        
                        // Set up grammar for dictation
                        var dictationGrammar = new DictationGrammar();
                        _speechEngine.LoadGrammar(dictationGrammar);
                        System.Diagnostics.Debug.WriteLine("Loaded dictation grammar");
                        
                        // Set up event handlers
                        _speechEngine.SpeechRecognized += SpeechEngine_SpeechRecognized;
                        _speechEngine.SpeechRecognitionRejected += SpeechEngine_SpeechRecognitionRejected;
                        _speechEngine.RecognizeCompleted += SpeechEngine_RecognizeCompleted;
                        System.Diagnostics.Debug.WriteLine("Set up event handlers");
                        
                        // Set audio input to default device
                        _speechEngine.SetInputToDefaultAudioDevice();
                        System.Diagnostics.Debug.WriteLine("Set audio input to default device");
                        
                        _isInitialized = true;
                        System.Diagnostics.Debug.WriteLine("Windows Speech Recognition initialized successfully");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Windows Speech Recognition initialization failed: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Speech recognition initialization failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StartRecognitionAsync()
        {
            if (!_isInitialized || _speechEngine == null)
            {
                RecognitionError?.Invoke(this, "Speech recognition not initialized");
                return false;
            }

            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        _speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                        _isListening = true;
                        System.Diagnostics.Debug.WriteLine("Windows Speech Recognition started");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to start speech recognition: {ex.Message}");
                        RecognitionError?.Invoke(this, $"Failed to start recognition: {ex.Message}");
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                RecognitionError?.Invoke(this, $"Start recognition failed: {ex.Message}");
                return false;
            }
        }

        public async Task StopRecognitionAsync()
        {
            if (_speechEngine != null && _isListening)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        _speechEngine.RecognizeAsyncStop();
                        _isListening = false;
                        System.Diagnostics.Debug.WriteLine("Windows Speech Recognition stopped");
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error stopping speech recognition: {ex.Message}");
                }
            }
        }

        public async Task<bool> ProcessAudioDataAsync(byte[] audioData)
        {
            // Windows Speech Recognition handles audio input directly
            // This method is called for compatibility but doesn't process the audio data
            // The recognition happens through the audio input device
            return await Task.FromResult(true);
        }

        private void SpeechEngine_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            try
            {
                if (e.Result != null && !string.IsNullOrWhiteSpace(e.Result.Text))
                {
                    var recognizedText = e.Result.Text.Trim();
                    System.Diagnostics.Debug.WriteLine($"Windows Speech Recognition: {recognizedText}");
                    SpeechRecognized?.Invoke(this, recognizedText);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in speech recognized event: {ex.Message}");
            }
        }

        private void SpeechEngine_SpeechRecognitionRejected(object? sender, SpeechRecognitionRejectedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Speech recognition rejected");
                // Don't invoke error event for rejected speech as it's normal
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in speech rejected event: {ex.Message}");
            }
        }

        private void SpeechEngine_RecognizeCompleted(object? sender, RecognizeCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Speech recognition error: {e.Error.Message}");
                    RecognitionError?.Invoke(this, $"Recognition error: {e.Error.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in recognize completed event: {ex.Message}");
            }
        }

        public async Task<bool> TestWithSampleAudioAsync()
        {
            try
            {
                // For Windows Speech Recognition, we can't easily test with sample audio
                // Instead, we'll test if the service is working by checking initialization
                return await Task.FromResult(_isInitialized);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test with sample audio failed: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_speechEngine != null)
                {
                    if (_isListening)
                    {
                        _speechEngine.RecognizeAsyncStop();
                    }
                    _speechEngine.Dispose();
                    _speechEngine = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing speech engine: {ex.Message}");
            }
        }

        public bool IsInitialized => _isInitialized;
        public bool IsListening => _isListening;
    }
}
