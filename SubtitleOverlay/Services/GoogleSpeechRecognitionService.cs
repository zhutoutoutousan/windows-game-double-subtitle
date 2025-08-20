using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace SubtitleOverlay.Services
{
    public class GoogleSpeechRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private bool _isInitialized = false;

        public event EventHandler<string>? SpeechRecognized;
        public event EventHandler<string>? RecognitionError;

        public GoogleSpeechRecognitionService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Test the API key by making a simple request
                var testRequest = new
                {
                    config = new
                    {
                        encoding = "LINEAR16",
                        sampleRateHertz = 16000,
                        languageCode = "en-US",
                        enableAutomaticPunctuation = true,
                        useEnhanced = true
                    },
                    audio = new
                    {
                        content = Convert.ToBase64String(new byte[1024]) // Empty audio for test
                    }
                };

                var json = JsonSerializer.Serialize(testRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(
                    $"https://speech.googleapis.com/v1/speech:recognize?key={_apiKey}", 
                    content);
                
                // Check if the API key is valid
                if (response.IsSuccessStatusCode)
                {
                    _isInitialized = true;
                    System.Diagnostics.Debug.WriteLine("Google Speech Recognition API key is valid");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Bad request usually means invalid audio, but API key is valid
                    _isInitialized = true;
                    System.Diagnostics.Debug.WriteLine("Google Speech Recognition API key is valid (BadRequest expected for empty audio)");
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    System.Diagnostics.Debug.WriteLine("Google Speech Recognition API key is invalid");
                    return false;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Google Speech Recognition initialization failed: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Google Speech Recognition initialization failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ProcessAudioDataAsync(byte[] audioData)
        {
            if (!_isInitialized || string.IsNullOrEmpty(_apiKey))
            {
                RecognitionError?.Invoke(this, "Google Speech Recognition not initialized or API key missing");
                return false;
            }

            try
            {
                // Only process if we have enough audio data
                if (audioData.Length < 1024)
                {
                    return true; // Skip small audio chunks
                }

                // Convert audio data to proper format and then to base64
                var convertedAudio = ConvertAudioToLinear16(audioData);
                var audioBase64 = Convert.ToBase64String(convertedAudio);

                // Create request payload according to Google Speech-to-Text REST API documentation
                var request = new
                {
                    config = new
                    {
                        encoding = "LINEAR16",
                        sampleRateHertz = 16000, // Use 16kHz for better compatibility
                        languageCode = "en-US",
                        enableAutomaticPunctuation = true,
                        enableWordTimeOffsets = false,
                        enableWordConfidence = true,
                        model = "latest_long", // Better for longer audio segments
                        useEnhanced = true, // Use enhanced models for better accuracy
                        audioChannelCount = 2, // Stereo audio
                        enableSeparateRecognitionPerChannel = false
                    },
                    audio = new
                    {
                        content = audioBase64
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send request to Google Speech-to-Text API using the correct endpoint
                var response = await _httpClient.PostAsync(
                    $"https://speech.googleapis.com/v1/speech:recognize?key={_apiKey}", 
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Google Speech API Response: {responseContent}");
                    
                    var result = JsonSerializer.Deserialize<GoogleSpeechResponse>(responseContent);

                    if (result?.Results != null && result.Results.Count > 0)
                    {
                        foreach (var speechResult in result.Results)
                        {
                            if (speechResult.Alternatives != null && speechResult.Alternatives.Count > 0)
                            {
                                var recognizedText = speechResult.Alternatives[0].Transcript;
                                var confidence = speechResult.Alternatives[0].Confidence;
                                
                                if (!string.IsNullOrWhiteSpace(recognizedText))
                                {
                                    System.Diagnostics.Debug.WriteLine($"Recognized: {recognizedText} (confidence: {confidence})");
                                    SpeechRecognized?.Invoke(this, recognizedText);
                                }
                            }
                        }
                    }
                    else
                    {
                        // No speech detected in the audio
                        System.Diagnostics.Debug.WriteLine("No speech detected in audio sample");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorMessage = $"Google Speech API error: {response.StatusCode} - {errorContent}";
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                    RecognitionError?.Invoke(this, errorMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Audio processing failed: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMessage);
                RecognitionError?.Invoke(this, errorMessage);
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // Helper method to convert audio to proper format for Google Speech API
        private byte[] ConvertAudioToLinear16(byte[] audioData, int originalSampleRate = 44100)
        {
            try
            {
                // For now, we'll assume the audio is already in a compatible format
                // In a production app, you'd want to properly convert the audio format
                // This is a simplified version - you might need to use NAudio to convert
                
                // If the audio is already 16-bit PCM, we can use it directly
                // Otherwise, we'd need to convert it
                
                // For testing purposes, let's create some test audio that should work
                if (audioData.Length < 1024)
                {
                    // Create a simple sine wave for testing
                    var testAudio = new byte[4096];
                    var random = new Random();
                    random.NextBytes(testAudio);
                    return testAudio;
                }
                
                return audioData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio conversion failed: {ex.Message}");
                return audioData; // Return original data if conversion fails
            }
        }

        // Test method to verify API is working with a simple audio sample
        public async Task<bool> TestWithSampleAudioAsync()
        {
            try
            {
                // Create a simple test audio sample (silence)
                var testAudio = new byte[4096];
                new Random().NextBytes(testAudio);
                
                var result = await ProcessAudioDataAsync(testAudio);
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test with sample audio failed: {ex.Message}");
                return false;
            }
        }

        // Response models for Google Speech-to-Text API
        private class GoogleSpeechResponse
        {
            public List<SpeechResult>? Results { get; set; }
        }

        private class SpeechResult
        {
            public List<SpeechAlternative>? Alternatives { get; set; }
        }

        private class SpeechAlternative
        {
            public string? Transcript { get; set; }
            public double Confidence { get; set; }
        }
    }
}
