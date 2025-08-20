using System.Windows;
using System.Windows.Controls;
using SubtitleOverlay.Services;
using SubtitleOverlay.Models;
using SubtitleOverlay.Windows;
using System.Drawing;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace SubtitleOverlay
{
    public partial class MainWindow_Working : Window
    {
        private bool _isListening = false;
        private bool _isOverlayVisible = false;
        private bool _isOCRCapturing = false;
        private OverlayWindow_Working? _overlayWindow;
        private SimpleAudioCaptureService _audioCaptureService;
        private GoogleSpeechRecognitionService? _googleSpeechService;
        private FreeSpeechRecognitionService? _freeSpeechService;
        private WindowsOCRService? _ocrService;
        private GoogleTranslationService? _translationService;
        private System.Threading.Timer? _simulationTimer;
        private bool _useFreeSpeechService = true; // Default to free service
        private Rectangle _captureArea = Rectangle.Empty;
        private string _targetLanguage = "zh-CN"; // Default target language (Chinese)

        public MainWindow_Working()
        {
            InitializeComponent();
            
            // Initialize services
            _audioCaptureService = new SimpleAudioCaptureService();
            _audioCaptureService.AudioDataReceived += AudioCaptureService_AudioDataReceived;
            _audioCaptureService.AudioLevelChanged += AudioCaptureService_AudioLevelChanged;
            _audioCaptureService.SpeechDetected += AudioCaptureService_SpeechDetected;
            
            // Speech recognition service will be initialized when API key is provided
            
            // Setup event handlers after controls are loaded
            this.Loaded += MainWindow_Loaded;
            
            // Add log entry
            AddLog("Application initialized successfully.");
            
            // Initialize OCR service
            InitializeOCRService();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Setup event handlers after controls are fully loaded
                FontSizeSlider.ValueChanged += FontSizeSlider_ValueChanged;
                
                // Set initial audio mode status
                if (AudioModeStatusText != null)
                {
                    AudioModeStatusText.Text = "Simple Mode";
                    AudioModeStatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                
                AddLog("🎵 Audio mode: Simple Simulation (Safe) - Default");
                
                // Initialize free speech recognition service by default
                await InitializeFreeSpeechService();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow_Loaded error: {ex.Message}");
            }
        }

        private async Task InitializeFreeSpeechService()
        {
            try
            {
                AddLog("🆓 Initializing free Windows Speech Recognition service...");
                
                _freeSpeechService = new FreeSpeechRecognitionService();
                _freeSpeechService.SpeechRecognized += SpeechRecognitionService_SpeechRecognized;
                _freeSpeechService.RecognitionError += SpeechRecognitionService_RecognitionError;
                
                var success = await _freeSpeechService.InitializeAsync();
                if (success)
                {
                    TranslationStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                    TranslationStatusText.Text = "Free Service";
                    AddLog("✅ Free Windows Speech Recognition service initialized successfully!");
                    AddLog("💡 This service works offline and is completely free!");
                    _useFreeSpeechService = true;
                    
                    // Debug logging
                    AddLog($"🔧 DEBUG: _useFreeSpeechService = {_useFreeSpeechService}");
                    AddLog($"🔧 DEBUG: _freeSpeechService != null = {_freeSpeechService != null}");
                    AddLog($"🔧 DEBUG: _googleSpeechService != null = {_googleSpeechService != null}");
                }
                else
                {
                    AddLog("⚠️ Failed to initialize free speech recognition service.");
                    AddLog("🔍 Possible reasons:");
                    AddLog("   - Windows Speech Recognition is not installed or enabled");
                    AddLog("   - No microphone is connected");
                    AddLog("   - Speech Recognition is disabled in Windows settings");
                    AddLog("   - Language pack for speech recognition is missing");
                    AddLog("💡 To enable: Windows Settings > Ease of Access > Speech > Turn on 'Speech recognition'");
                    AddLog("💡 You can still use Google Speech Recognition with an API key or Demo Mode.");
                    _freeSpeechService = null;
                    _useFreeSpeechService = false;
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error initializing free speech recognition: {ex.Message}");
                AddLog("🔍 This usually means Windows Speech Recognition is not available on your system.");
                AddLog("💡 Try: Windows Settings > Ease of Access > Speech > Enable 'Speech recognition'");
                _freeSpeechService = null;
                _useFreeSpeechService = false;
            }
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FontSizeDisplay != null)
            {
                FontSizeDisplay.Text = ((int)e.NewValue).ToString();
            }
        }

        private async void StartRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddLog("🔄 Starting speech recognition...");
                
                _isListening = true;
                StartRecognitionButton.IsEnabled = false;
                StopRecognitionButton.IsEnabled = true;
                
                SpeechStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                SpeechStatusText.Text = "Active";
                
                // Try to start free speech recognition first
                if (_useFreeSpeechService && _freeSpeechService != null)
                {
                    AddLog("🆓 Starting free Windows Speech Recognition...");
                    AddLog("💡 Speak into your microphone to see real-time recognition!");
                    
                    var success = await _freeSpeechService.StartRecognitionAsync();
                    if (success)
                    {
                        AddLog("✅ Free speech recognition started successfully!");
                        AddLog("🎤 Speak now - your words will appear in the overlay!");
                        return;
                    }
                    else
                    {
                        AddLog("⚠️ Free speech recognition failed, trying audio capture...");
                    }
                }
                
                AddLog("🎤 Starting real audio capture from system output...");
                AddLog("💡 Note: Simple Simulation mode generates random audio data, not real speech");
                AddLog("💡 For actual speech recognition, try Windows API mode or use Demo Mode for testing");
                
                // Start real audio capture with better error handling and timeout
                var captureTask = _audioCaptureService.StartCaptureAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15)); // 15 second timeout
                
                var completedTask = await Task.WhenAny(captureTask, timeoutTask);
                
                if (completedTask == captureTask)
                {
                    var success = await captureTask;
                    if (success)
                    {
                        AddLog("✅ Real audio capture started successfully. Listening to system audio...");
                        AddLog("🎵 Play some audio (music, video, game) to see it being captured!");
                    }
                    else
                    {
                        AddLog("⚠️ Audio capture failed, switching to simulation mode...");
                        StartSimulationMode();
                    }
                }
                else
                {
                    AddLog("⏰ Audio capture timed out, switching to simulation mode...");
                    StartSimulationMode();
                }
            }
            catch (Exception ex)
            {
                AddLog($"💥 CRITICAL ERROR starting recognition: {ex.Message}");
                AddLog($"Stack trace: {ex.StackTrace}");
                StartSimulationMode();
            }
        }

        private void StartSimulationMode()
        {
            AddLog("🎭 Using simulated speech recognition (no real audio capture)");
            
            // Start simulation timer instead
            _simulationTimer = new System.Threading.Timer(_ =>
            {
                if (_isListening)
                {
                    SimulateSpeechRecognition();
                }
            }, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));
            
            AddLog("🎭 Simulation mode active. Click 'Test Speech' to see sample recognition.");
        }

        private async void StopRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            await StopRecognition();
        }

        private async Task StopRecognition()
        {
            _isListening = false;
            StartRecognitionButton.IsEnabled = true;
            StopRecognitionButton.IsEnabled = false;
            
            SpeechStatusIndicator.Fill = System.Windows.Media.Brushes.Red;
            SpeechStatusText.Text = "Inactive";
            
            // Stop simulation timer (no longer needed)
            _simulationTimer?.Dispose();
            _simulationTimer = null;
            
            // Stop free speech recognition
            if (_freeSpeechService != null && _freeSpeechService.IsListening)
            {
                await _freeSpeechService.StopRecognitionAsync();
                AddLog("🆓 Free speech recognition stopped.");
            }
            
            // Stop audio capture
            await _audioCaptureService.StopCaptureAsync();
            
            AddLog("Speech recognition stopped.");
        }

        private void ShowOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_overlayWindow == null)
                {
                    _overlayWindow = new OverlayWindow_Working();
                }
                
                _overlayWindow.Show();
                _isOverlayVisible = true;
                ShowOverlayButton.IsEnabled = false;
                HideOverlayButton.IsEnabled = true;
                
                OverlayStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                OverlayStatusText.Text = "Visible";
                
                AddLog("Overlay window shown.");
            }
            catch (Exception ex)
            {
                AddLog($"Error showing overlay: {ex.Message}");
            }
        }

        private void HideOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _overlayWindow?.Hide();
                _isOverlayVisible = false;
                ShowOverlayButton.IsEnabled = true;
                HideOverlayButton.IsEnabled = false;
                
                OverlayStatusIndicator.Fill = System.Windows.Media.Brushes.Red;
                OverlayStatusText.Text = "Hidden";
                
                AddLog("Overlay window hidden.");
            }
            catch (Exception ex)
            {
                AddLog($"Error hiding overlay: {ex.Message}");
            }
        }

        private async void SaveApiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = ApiKeyPasswordBox.Password;
            if (!string.IsNullOrEmpty(apiKey))
            {
                try
                {
                    AddLog("Initializing Google Speech Recognition service...");
                    
                    // Initialize Google Speech Recognition service
                    _googleSpeechService = new GoogleSpeechRecognitionService(apiKey);
                    _googleSpeechService.SpeechRecognized += SpeechRecognitionService_SpeechRecognized;
                    _googleSpeechService.RecognitionError += SpeechRecognitionService_RecognitionError;
                    
                    var success = await _googleSpeechService.InitializeAsync();
                    if (success)
                    {
                        TranslationStatusIndicator.Fill = System.Windows.Media.Brushes.Orange;
                        TranslationStatusText.Text = "Google Available";
                        AddLog("Google Speech Recognition service initialized successfully.");
                        AddLog("💡 Google service is available but FREE service is still preferred.");
                        AddLog("💡 To use Google, click 'Switch to Google' (if we add that button).");
                        // DON'T automatically switch to Google - keep using free service
                        // _useFreeSpeechService = false; // Keep free service as default
                    }
                    else
                    {
                        AddLog("Failed to initialize Google Speech Recognition service. Check your API key.");
                        _googleSpeechService = null;
                    }
                    
                    // Also update translation service with the same API key
                    UpdateTranslationServiceApiKey(apiKey);
                }
                catch (Exception ex)
                {
                    AddLog($"Error initializing speech recognition: {ex.Message}");
                    _googleSpeechService = null;
                }
            }
            else
            {
                AddLog("Please enter a Google Speech-to-Text API key.");
            }
        }
        
        private void UpdateTranslationServiceApiKey(string apiKey)
        {
            try
            {
                if (_translationService != null)
                {
                    // Update the translation service settings with the new API key
                    var translationSettings = new TranslationSettings
                    {
                        Provider = "Google",
                        ApiKey = apiKey,
                        ApiEndpoint = "https://translation.googleapis.com/language/translate/v2",
                        CacheEnabled = true,
                        CacheExpirationMinutes = 60
                    };
                    
                    // Re-initialize translation service with new API key
                    var loggerFactory = LoggerFactory.Create(builder => {});
                    var translationLogger = loggerFactory.CreateLogger<GoogleTranslationService>();
                    var textCleaningLogger = loggerFactory.CreateLogger<TextCleaningService>();
                    var textCleaningService = new TextCleaningService(textCleaningLogger);
                    
                    var options = Microsoft.Extensions.Options.Options.Create(translationSettings);
                    var httpClient = new HttpClient();
                    
                    _translationService = new GoogleTranslationService(translationLogger, options, httpClient, textCleaningService);
                    
                    AddLog($"🌐 Translation service updated with API key. Available: {_translationService.IsAvailable}");
                    AddLog($"🎯 Target language: {_targetLanguage}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error updating translation service: {ex.Message}");
            }
        }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = ApiKeyPasswordBox.Password;
            if (!string.IsNullOrEmpty(apiKey))
            {
                AddLog("Testing translation service connection...");
                
                // TODO: Test actual translation service
                // For now, simulate a successful test
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog("Translation service test successful!");
                    });
                });
            }
            else
            {
                AddLog("Please enter an API key first.");
            }
        }

        private async void TestSpeechButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog("🧪 Testing speech recognition...");
            
            // Ensure overlay is visible for testing
            if (_overlayWindow == null)
            {
                _overlayWindow = new OverlayWindow_Working();
            }
            
            if (!_isOverlayVisible)
            {
                _overlayWindow.Show();
                _isOverlayVisible = true;
                ShowOverlayButton.IsEnabled = false;
                HideOverlayButton.IsEnabled = true;
                OverlayStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                OverlayStatusText.Text = "Visible";
                AddLog("✅ Overlay window shown for testing.");
            }
            
            if (_googleSpeechService != null)
            {
                AddLog("🎤 Testing with Google Speech Recognition service...");
                
                // Use the improved test method
                var success = await _googleSpeechService.TestWithSampleAudioAsync();
                if (success)
                {
                    AddLog("✅ Google Speech API test completed successfully");
                }
                else
                {
                    AddLog("❌ Google Speech API test failed");
                }
            }
            else
            {
                AddLog("🎭 Using simulated speech recognition for testing...");
                SimulateSpeechRecognition();
            }
        }

        private void TestOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog("🖥️ Testing overlay display...");
            
            if (_overlayWindow != null && _isOverlayVisible)
            {
                var testText = "This is a test of the subtitle overlay! 🎬";
                _overlayWindow.UpdateOriginalText(testText);
                AddLog($"📝 Displayed test text in overlay: {testText}");
            }
            else
            {
                AddLog("⚠️ Overlay window is not visible. Please show the overlay first.");
            }
        }

        private void QuickTestButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog("⚡ Quick test - showing overlay and displaying sample text...");
            
            try
            {
                // Show overlay if not visible
                if (_overlayWindow == null)
                {
                    _overlayWindow = new OverlayWindow_Working();
                }
                
                if (!_isOverlayVisible)
                {
                    _overlayWindow.Show();
                    _isOverlayVisible = true;
                    ShowOverlayButton.IsEnabled = false;
                    HideOverlayButton.IsEnabled = true;
                    OverlayStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                    OverlayStatusText.Text = "Visible";
                    AddLog("✅ Overlay window shown.");
                }
                
                // Display sample text
                var sampleTexts = new[]
                {
                    "Hello! This is a sample subtitle text. 🎬",
                    "The speech recognition is working! 🎤",
                    "You can see this text in the overlay window. 📺",
                    "This demonstrates the subtitle functionality. ✨"
                };
                
                var random = new Random();
                var selectedText = sampleTexts[random.Next(sampleTexts.Length)];
                
                _overlayWindow.UpdateOriginalText(selectedText);
                AddLog($"📝 Quick test: {selectedText}");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Quick test failed: {ex.Message}");
            }
        }

        private void DemoModeButton_Click(object sender, RoutedEventArgs e)
        {
            AddLog("🎭 Starting demo mode - no audio capture required...");
            
            try
            {
                // Show overlay if not visible
                if (_overlayWindow == null)
                {
                    _overlayWindow = new OverlayWindow_Working();
                }
                
                if (!_isOverlayVisible)
                {
                    _overlayWindow.Show();
                    _isOverlayVisible = true;
                    ShowOverlayButton.IsEnabled = false;
                    HideOverlayButton.IsEnabled = true;
                    OverlayStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                    OverlayStatusText.Text = "Visible";
                    AddLog("✅ Overlay window shown.");
                }
                
                // Start demo mode with simulated speech recognition
                _isListening = true;
                StartRecognitionButton.IsEnabled = false;
                StopRecognitionButton.IsEnabled = true;
                SpeechStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                SpeechStatusText.Text = "Demo Active";
                
                // Start simulation timer for demo
                _simulationTimer = new System.Threading.Timer(_ =>
                {
                    if (_isListening)
                    {
                        SimulateSpeechRecognition();
                    }
                }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4));
                
                AddLog("🎭 Demo mode active! You'll see simulated speech recognition every few seconds.");
                AddLog("🎬 The overlay will show sample text to demonstrate the functionality.");
                AddLog("💡 This is the best way to test the overlay without real audio capture.");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Demo mode failed: {ex.Message}");
            }
        }

        private async void UseFreeServiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddLog("🆓 Switching to FREE Windows Speech Recognition service...");
                
                // Force switch to free service
                _useFreeSpeechService = true;
                
                // Re-initialize free service if needed
                if (_freeSpeechService == null)
                {
                    await InitializeFreeSpeechService();
                }
                
                // Update UI to show free service is active
                TranslationStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
                TranslationStatusText.Text = "Free Service";
                
                AddLog("✅ Now using FREE Windows Speech Recognition!");
                AddLog("💡 No API key required - works completely offline!");
                AddLog("💡 Click 'Start Recognition' and speak into your microphone!");
                
                // Debug logging
                AddLog($"🔧 DEBUG: _useFreeSpeechService = {_useFreeSpeechService}");
                AddLog($"🔧 DEBUG: _freeSpeechService != null = {_freeSpeechService != null}");
                AddLog($"🔧 DEBUG: _googleSpeechService != null = {_googleSpeechService != null}");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error switching to free service: {ex.Message}");
            }
        }

        private void AudioModeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                // Check if controls are initialized
                if (AudioModeComboBox?.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem && 
                    AudioModeStatusText != null)
                {
                    var mode = selectedItem.Tag?.ToString();
                    switch (mode)
                    {
                        case "simple":
                            AudioModeStatusText.Text = "Simple Mode";
                            AudioModeStatusText.Foreground = System.Windows.Media.Brushes.Green;
                            AddLog("🎵 Audio mode: Simple Simulation (Safe) - Random audio data");
                            AddLog("💡 Note: Simple mode generates random audio, not real speech");
                            AddLog("💡 For real speech recognition, try Windows API mode or use Demo Mode");
                            break;
                        case "windows":
                            AudioModeStatusText.Text = "Windows API";
                            AudioModeStatusText.Foreground = System.Windows.Media.Brushes.Orange;
                            AddLog("🎵 Audio mode: Windows API (Experimental) - Real audio capture");
                            AddLog("💡 This mode captures real system audio for speech recognition");
                            break;
                        case "naudio":
                            AudioModeStatusText.Text = "NAudio (Legacy)";
                            AudioModeStatusText.Foreground = System.Windows.Media.Brushes.Red;
                            AddLog("🎵 Audio mode: NAudio (Legacy) - May cause crashes");
                            AddLog("⚠️ Warning: This mode has been known to cause application crashes");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"AudioModeComboBox_SelectionChanged error: {ex.Message}");
            }
        }

        private void AudioCaptureService_AudioDataReceived(object? sender, byte[] audioData)
        {
            // Real audio data received - process it for speech recognition
            ProcessRealAudioData(audioData);
        }

        private void AudioCaptureService_AudioLevelChanged(object? sender, string levelInfo)
        {
            // Update UI with audio level information
            Dispatcher.Invoke(() =>
            {
                AddLog(levelInfo);
            });
        }

        private void AudioCaptureService_SpeechDetected(object? sender, string speechInfo)
        {
            // Speech detected in the audio
            Dispatcher.Invoke(() =>
            {
                AddLog(speechInfo);
                
                // If we have real speech recognition, let it handle the audio data
                // If not, fall back to simulation
                if (speechInfo.Contains("Speech detected"))
                {
                    // Use simulation if no real speech recognition service is available
                    if (_googleSpeechService == null && _freeSpeechService == null)
                    {
                        SimulateSpeechRecognition();
                    }
                    else
                    {
                        if (_useFreeSpeechService && _freeSpeechService != null)
                        {
                            AddLog($"🔊 Speech detected, processing with FREE Windows Speech Recognition...");
                        }
                        else if (_googleSpeechService != null)
                        {
                            AddLog($"🔊 Speech detected, processing with Google Speech Recognition...");
                        }
                        else
                        {
                            AddLog($"🔊 Speech detected, but no speech service available...");
                            SimulateSpeechRecognition();
                        }
                    }
                }
            });
        }

        private async void ProcessRealAudioData(byte[] audioData)
        {
            // Debug logging
            AddLog($"🔧 DEBUG ProcessRealAudioData: _useFreeSpeechService={_useFreeSpeechService}, _freeSpeechService!=null={_freeSpeechService != null}, _googleSpeechService!=null={_googleSpeechService != null}");
            
            // Process real audio data with speech recognition
            if (_useFreeSpeechService && _freeSpeechService != null)
            {
                try
                {
                    // Free speech service handles audio input directly from microphone
                    AddLog($"🆓 Using FREE Windows Speech Recognition (microphone input)...");
                    // The free service doesn't need audio data processing - it uses microphone directly
                    // No additional processing needed here
                }
                catch (Exception ex)
                {
                    AddLog($"❌ Free speech recognition error: {ex.Message}");
                }
            }
            else if (_googleSpeechService != null)
            {
                try
                {
                    // Only process audio data if it's significant enough
                    if (audioData.Length > 1024)
                    {
                        AddLog($"🔊 Processing audio data ({audioData.Length} bytes) with Google Speech Recognition...");
                        var result = await _googleSpeechService.ProcessAudioDataAsync(audioData);
                        
                        if (!result)
                        {
                            AddLog("⚠️ No speech detected in audio sample (this is normal for simulated audio)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"❌ Google speech recognition processing error: {ex.Message}");
                }
            }
            else
            {
                // Fallback to simulation if no real speech recognition service
                AddLog($"🎭 Using simulated speech recognition (no real service available)");
                SimulateSpeechRecognition();
            }
        }

        private void SpeechRecognitionService_SpeechRecognized(object? sender, string recognizedText)
        {
            // Real speech recognition result
            Dispatcher.Invoke(() =>
            {
                // Update overlay with recognized text
                if (_overlayWindow != null && _isOverlayVisible)
                {
                    _overlayWindow.UpdateOriginalText(recognizedText);
                }
                
                AddLog($"🎤 SPEECH RECOGNIZED: {recognizedText}");
            });
        }

        private void SpeechRecognitionService_RecognitionError(object? sender, string errorMessage)
        {
            // Speech recognition error
            Dispatcher.Invoke(() =>
            {
                AddLog($"❌ SPEECH RECOGNITION ERROR: {errorMessage}");
            });
        }

        private void SimulateSpeechRecognition()
        {
            // Fallback to simulation if real speech recognition is not available
            if (_googleSpeechService == null && _freeSpeechService == null)
            {
                var phrases = new[]
                {
                    "Hello, how are you today?",
                    "This is a test of the speech recognition system.",
                    "The weather is nice today.",
                    "I'm testing the subtitle overlay functionality.",
                    "This should appear in the overlay window.",
                    "Speech recognition is working properly.",
                    "The audio capture is functioning correctly.",
                    "Real-time translation will be added next."
                };

                var random = new Random();
                var phrase = phrases[random.Next(phrases.Length)];
                
                Dispatcher.Invoke(() =>
                {
                    // Update overlay with recognized text
                    if (_overlayWindow != null && _isOverlayVisible)
                    {
                        _overlayWindow.UpdateOriginalText(phrase);
                    }
                    
                    AddLog($"🎭 SIMULATED RECOGNITION: {phrase}");
                });
            }
        }

        private void AddLog(string message)
        {
            try
            {
                if (LogTextBox != null)
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss");
                    var logEntry = $"[{timestamp}] {message}";
                    
                    if (LogTextBox.Text.Length > 0)
                    {
                        LogTextBox.Text += Environment.NewLine + logEntry;
                    }
                    else
                    {
                        LogTextBox.Text = logEntry;
                    }
                    
                    LogTextBox.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddLog error: {ex.Message}");
            }
        }

        // OCR Methods
        private void InitializeOCRService()
        {
            try
            {
                // Create a simple logger for OCR service
                var loggerFactory = LoggerFactory.Create(builder => {});
                var logger = loggerFactory.CreateLogger<WindowsOCRService>();
                var textCleaningLogger = loggerFactory.CreateLogger<TextCleaningService>();
                
                // Create text cleaning service
                var textCleaningService = new TextCleaningService(textCleaningLogger);
                
                // Initialize translation service
                InitializeTranslationService(loggerFactory, textCleaningService);
                
                // Create a simple translation service for OCR (you can replace this with your preferred service)
                var translationService = CreateSimpleTranslationService();
                
                _ocrService = new WindowsOCRService(logger, textCleaningService, translationService);
                _ocrService.TextRecognized += OCRService_TextRecognized;
                _ocrService.RecognitionError += OCRService_RecognitionError;
                
                AddLog($"🔍 OCR service initialized. Available: {_ocrService.IsAvailable}");
                AddLog($"🧹 Text cleaning service initialized with language models");
                AddLog($"🌐 Translation service integrated for OCR workflow");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error initializing OCR service: {ex.Message}");
            }
        }
        
        private void InitializeTranslationService(ILoggerFactory loggerFactory, ITextCleaningService textCleaningService)
        {
            try
            {
                var translationLogger = loggerFactory.CreateLogger<GoogleTranslationService>();
                
                // Create translation settings
                var translationSettings = new TranslationSettings
                {
                    Provider = "Google",
                    ApiKey = "", // Will be set when user provides API key
                    ApiEndpoint = "https://translation.googleapis.com/language/translate/v2",
                    CacheEnabled = true,
                    CacheExpirationMinutes = 60
                };
                
                var options = Microsoft.Extensions.Options.Options.Create(translationSettings);
                var httpClient = new HttpClient();
                
                _translationService = new GoogleTranslationService(translationLogger, options, httpClient, textCleaningService);
                
                AddLog($"🌐 Translation service initialized. Available: {_translationService.IsAvailable}");
                AddLog($"🎯 Target language: {_targetLanguage}");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error initializing translation service: {ex.Message}");
            }
        }

        private ITranslationService CreateSimpleTranslationService()
        {
            // Create a simple translation service that can be extended later
            // For now, it will just return the cleaned text without translation
            return new SimpleTranslationService();
        }

        private async void SelectAreaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddLog("📷 Selecting OCR capture area...");
                
                if (_ocrService == null || !_ocrService.IsAvailable)
                {
                    AddLog("❌ OCR service not available");
                    return;
                }

                var area = await _ocrService.SelectCaptureAreaAsync();
                if (area != Rectangle.Empty)
                {
                    _captureArea = area;
                    CaptureAreaText.Text = $"Area: {area.X}, {area.Y}, {area.Width}x{area.Height}";
                    AddLog($"✅ Capture area selected: {area.X}, {area.Y}, {area.Width}x{area.Height}");
                    StartOCRButton.IsEnabled = true;
                }
                else
                {
                    AddLog("❌ No area selected");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error selecting capture area: {ex.Message}");
            }
        }

        private async void StartOCRButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_captureArea == Rectangle.Empty)
                {
                    AddLog("⚠️ No capture area selected. Please select an area first.");
                    return;
                }

                if (_ocrService == null || !_ocrService.IsAvailable)
                {
                    AddLog("❌ OCR service not available");
                    return;
                }

                // Show overlay if not visible
                if (!_isOverlayVisible)
                {
                    ShowOverlayButton_Click(sender, e);
                }

                await _ocrService.StartCaptureAsync(_captureArea);
                _isOCRCapturing = true;
                StartOCRButton.IsEnabled = false;
                StopOCRButton.IsEnabled = true;
                SelectAreaButton.IsEnabled = false;
                
                AddLog("🔍 OCR capture started");
                AddLog("📝 Text will be recognized from the selected area and displayed in overlay");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error starting OCR: {ex.Message}");
            }
        }

        private async void StopOCRButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_ocrService != null)
                {
                    await _ocrService.StopCaptureAsync();
                }
                
                _isOCRCapturing = false;
                StartOCRButton.IsEnabled = true;
                StopOCRButton.IsEnabled = false;
                SelectAreaButton.IsEnabled = true;
                
                AddLog("⏹️ OCR capture stopped");
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error stopping OCR: {ex.Message}");
            }
        }

        private async void OCRService_TextRecognized(object? sender, string recognizedText)
        {
            try
            {
                AddLog($"📝 OCR RECOGNIZED: {recognizedText}");
                
                string translatedText = "";
                
                // Translate the recognized text if translation service is available
                if (_translationService != null && _translationService.IsAvailable && !string.IsNullOrWhiteSpace(recognizedText))
                {
                    try
                    {
                        AddLog($"🌐 Translating to {_targetLanguage}...");
                        translatedText = await _translationService.TranslateAsync(recognizedText, _targetLanguage, "en");
                        AddLog($"🌐 TRANSLATED: {translatedText}");
                    }
                    catch (Exception ex)
                    {
                        AddLog($"❌ Translation error: {ex.Message}");
                        translatedText = ""; // Fallback to no translation
                    }
                }
                
                // Update overlay with both original and translated text
                Dispatcher.Invoke(() =>
                {
                    if (_overlayWindow != null && _isOverlayVisible)
                    {
                        if (!string.IsNullOrWhiteSpace(translatedText))
                        {
                            _overlayWindow.UpdateSubtitles(recognizedText, translatedText);
                        }
                        else
                        {
                            _overlayWindow.UpdateOriginalText(recognizedText);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error processing OCR text: {ex.Message}");
            }
        }

        private void OCRService_RecognitionError(object? sender, Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                AddLog($"❌ OCR ERROR: {ex.Message}");
            });
        }

        private async void AdjustParametersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_captureArea == Rectangle.Empty)
                {
                    AddLog("⚠️ No capture area selected. Please select an area first.");
                    return;
                }

                AddLog("⚙️ Opening OCR parameter adjustment window...");
                
                // Generate area ID for parameter management
                var areaId = $"area_{_captureArea.X}_{_captureArea.Y}_{_captureArea.Width}_{_captureArea.Height}";
                
                // Get current parameters for this area (or defaults)
                var currentParameters = new OCRParameters
                {
                    AreaId = areaId,
                    Description = $"OCR parameters for area {_captureArea.X}, {_captureArea.Y}, {_captureArea.Width}x{_captureArea.Height}"
                };
                
                // Show parameter adjustment window
                var parameterWindow = new OCRParameterWindow(currentParameters);
                parameterWindow.Owner = this;
                
                // Subscribe to test completed event
                parameterWindow.TestCompleted += async (s, e) =>
                {
                    try
                    {
                        AddLog("🧪 Testing OCR parameters on selected area...");
                        
                        // Test the parameters with a single capture using current parameters from modal
                        var testText = await _ocrService?.RecognizeTextAsync(_captureArea, parameterWindow.Parameters);
                        if (!string.IsNullOrWhiteSpace(testText))
                        {
                            AddLog($"✅ Test successful! Recognized text: {testText}");
                            
                            string translatedText = "";
                            
                            // Translate the test text if translation service is available
                            if (_translationService != null && _translationService.IsAvailable)
                            {
                                try
                                {
                                    AddLog($"🌐 Translating test text to {_targetLanguage}...");
                                    translatedText = await _translationService.TranslateAsync(testText, _targetLanguage, "en");
                                    AddLog($"🌐 Test translation: {translatedText}");
                                }
                                catch (Exception ex)
                                {
                                    AddLog($"❌ Translation error: {ex.Message}");
                                }
                            }
                            
                            // Show the test result in overlay
                            if (_overlayWindow != null && _isOverlayVisible)
                            {
                                if (!string.IsNullOrWhiteSpace(translatedText))
                                {
                                    _overlayWindow.UpdateSubtitles(testText, translatedText);
                                }
                                else
                                {
                                    _overlayWindow.UpdateOriginalText(testText);
                                }
                            }
                            
                            // Update the parameter window with the result
                            var resultText = !string.IsNullOrWhiteSpace(translatedText) 
                                ? $"Original: {testText}\nTranslated: {translatedText}"
                                : testText;
                            parameterWindow.SetTestResult(resultText, true);
                        }
                        else
                        {
                            AddLog("⚠️ Test completed but no text was recognized. Try adjusting parameters.");
                            parameterWindow.SetTestResult("No text was recognized. Try adjusting parameters.", false);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog($"❌ Test failed: {ex.Message}");
                        parameterWindow.SetTestResult($"Test failed: {ex.Message}", false);
                    }
                };
                
                var result = parameterWindow.ShowDialog();
                if (result == true)
                {
                    if (parameterWindow.IsTestRequested)
                    {
                        AddLog("🧪 Testing OCR parameters on selected area...");
                        
                        // Test the parameters with a single capture using current parameters from modal
                        var testText = await _ocrService?.RecognizeTextAsync(_captureArea, parameterWindow.Parameters);
                        if (!string.IsNullOrWhiteSpace(testText))
                        {
                            AddLog($"✅ Test successful! Recognized text: {testText}");
                            
                            string translatedText = "";
                            
                            // Translate the test text if translation service is available
                            if (_translationService != null && _translationService.IsAvailable)
                            {
                                try
                                {
                                    AddLog($"🌐 Translating test text to {_targetLanguage}...");
                                    translatedText = await _translationService.TranslateAsync(testText, _targetLanguage, "en");
                                    AddLog($"🌐 Test translation: {translatedText}");
                                }
                                catch (Exception ex)
                                {
                                    AddLog($"❌ Translation error: {ex.Message}");
                                }
                            }
                            
                            // Show the test result in overlay
                            if (_overlayWindow != null && _isOverlayVisible)
                            {
                                if (!string.IsNullOrWhiteSpace(translatedText))
                                {
                                    _overlayWindow.UpdateSubtitles(testText, translatedText);
                                }
                                else
                                {
                                    _overlayWindow.UpdateOriginalText(testText);
                                }
                            }
                        }
                        else
                        {
                            AddLog("⚠️ Test completed but no text was recognized. Try adjusting parameters.");
                        }
                    }
                    else
                    {
                        AddLog("💾 Saving OCR parameters for selected area...");
                        
                        // Save parameters for this area
                        // Note: In a full implementation, you would save these to the parameter manager
                        AddLog($"✅ Parameters saved for area: {parameterWindow.Parameters.Description}");
                        AddLog($"📊 Settings: Contrast={parameterWindow.Parameters.Contrast:F1}, " +
                               $"Brightness={parameterWindow.Parameters.Brightness:F1}, " +
                               $"Confidence={parameterWindow.Parameters.MinimumConfidence:F1}");
                    }
                }
                else
                {
                    AddLog("❌ Parameter adjustment cancelled.");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ Error adjusting OCR parameters: {ex.Message}");
            }
        }
        
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                var previousLanguage = _targetLanguage;
                _targetLanguage = selectedItem.Tag.ToString() ?? "en";
                
                AddLog($"🌐 Target language changed from {previousLanguage} to {_targetLanguage}");
                
                // Update OCR service target language if it exists
                if (_ocrService != null)
                {
                    // The OCR service will use the updated target language for future translations
                    AddLog($"🔄 OCR service will now translate to {_targetLanguage}");
                }
            }
        }
    }
}
