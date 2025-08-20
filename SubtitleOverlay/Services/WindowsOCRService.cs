using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using SubtitleOverlay.Models;
using SubtitleOverlay.Services;
using DrawingRectangle = System.Drawing.Rectangle;
using WpfRectangle = System.Windows.Shapes.Rectangle;

namespace SubtitleOverlay.Services
{
    public class WindowsOCRService : IOCRService
    {
        private readonly ILogger<WindowsOCRService> _logger;
        private readonly ITextCleaningService _textCleaningService;
        private readonly ITranslationService _translationService;
        private readonly Timer _captureTimer;
        private DrawingRectangle _currentCaptureArea;
        private bool _isCapturing;
        private OCRParameters _currentParameters;

        public event EventHandler<string>? TextRecognized;
        public event EventHandler<Exception>? RecognitionError;

        public bool IsAvailable { get; private set; }
        public bool IsCapturing => _isCapturing;

        public WindowsOCRService(ILogger<WindowsOCRService> logger, ITextCleaningService textCleaningService, ITranslationService translationService)
        {
            _logger = logger;
            _textCleaningService = textCleaningService;
            _translationService = translationService;
            _captureTimer = new Timer(CaptureAndRecognize, null, Timeout.Infinite, Timeout.Infinite);
            _currentParameters = new OCRParameters(); // Use default parameters for now
            
            // Initialize availability synchronously
            CheckAvailabilitySync();
        }

        private void CheckAvailabilitySync()
        {
            try
            {
                _logger.LogInformation("🔍 Checking Windows OCR availability...");
                
                // Check if Windows OCR is available
                var languages = OcrEngine.AvailableRecognizerLanguages;
                _logger.LogInformation($"📋 Found {languages.Count} available OCR languages");
                
                IsAvailable = languages.Count > 0;
                _logger.LogInformation($"✅ Windows OCR available: {IsAvailable}");
                
                if (IsAvailable)
                {
                    _logger.LogInformation("🎉 Windows OCR engine initialized successfully");
                    foreach (var language in languages)
                    {
                        _logger.LogInformation($"🌐 Available language: {language.LanguageTag}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ No languages found in AvailableRecognizerLanguages, trying alternative method...");
                    
                    // Try to create an OCR engine directly to see if it works
                    var testEngine = OcrEngine.TryCreateFromUserProfileLanguages();
                    if (testEngine != null)
                    {
                        IsAvailable = true;
                        _logger.LogInformation("🎉 Windows OCR engine created successfully from user profile languages");
                    }
                    else
                    {
                        _logger.LogError("❌ Failed to create OCR engine from user profile languages");
                        _logger.LogWarning("📝 No OCR languages available. Please install language packs for OCR.");
                        _logger.LogWarning("🔧 To enable OCR, go to Windows Settings > Time & Language > Language & Region");
                        _logger.LogWarning("🔧 Add a language and make sure to install the language pack with OCR support.");
                        _logger.LogWarning("🔧 Windows OCR requires Windows 10 version 1803 or later with language packs installed.");
                    }
                }
            }
            catch (Exception ex)
            {
                IsAvailable = false;
                _logger.LogError($"❌ Windows OCR not available: {ex.Message}");
                _logger.LogError($"🔍 Exception type: {ex.GetType().Name}");
                _logger.LogError($"🔍 Stack trace: {ex.StackTrace}");
                _logger.LogWarning("⚠️ This might be due to missing Windows OCR components or language packs.");
                _logger.LogWarning("⚠️ Windows OCR requires Windows 10 version 1803 or later with language packs installed.");
            }
        }

        public async Task StartCaptureAsync(DrawingRectangle captureArea)
        {
            // Re-check availability before starting
            if (!IsAvailable)
            {
                CheckAvailabilitySync();
                if (!IsAvailable)
                {
                    throw new InvalidOperationException("Windows OCR is not available on this system.");
                }
            }

            _currentCaptureArea = captureArea;
            _isCapturing = true;
            _captureTimer.Change(0, 1000); // Capture every second
            _logger.LogInformation($"Started OCR capture for area: {captureArea}");
        }

        public async Task StopCaptureAsync()
        {
            _isCapturing = false;
            _captureTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _logger.LogInformation("Stopped OCR capture");
        }

        public async Task<string> RecognizeTextAsync(DrawingRectangle captureArea, OCRParameters? parameters = null)
        {
            try
            {
                // Use provided parameters or current parameters
                var currentParams = parameters ?? _currentParameters;
                
                using var bitmap = CaptureScreenArea(captureArea);
                if (bitmap == null) return string.Empty;

                var text = await RecognizeTextFromBitmapAsync(bitmap, currentParams);
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error recognizing text: {ex.Message}");
                RecognitionError?.Invoke(this, ex);
                return string.Empty;
            }
        }
        
        public void UpdateParameters(OCRParameters parameters)
        {
            _currentParameters = parameters;
            _logger.LogInformation($"Updated OCR parameters: Contrast={parameters.Contrast}, Brightness={parameters.Brightness}, Confidence={parameters.MinimumConfidence}");
        }

        public async Task<DrawingRectangle> SelectCaptureAreaAsync()
        {
            // Re-check availability before selecting area
            if (!IsAvailable)
            {
                CheckAvailabilitySync();
                if (!IsAvailable)
                {
                    _logger.LogError("OCR service not available for area selection");
                    return DrawingRectangle.Empty;
                }
            }

            var tcs = new TaskCompletionSource<DrawingRectangle>();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var selectionWindow = new AreaSelectionWindow();
                    selectionWindow.AreaSelected += (area) => tcs.SetResult(area);
                    selectionWindow.SelectionCancelled += () => tcs.SetResult(DrawingRectangle.Empty);
                    selectionWindow.Show();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            
            return await tcs.Task;
        }

        private async void CaptureAndRecognize(object? state)
        {
            if (!_isCapturing) return;

            try
            {
                var text = await RecognizeTextAsync(_currentCaptureArea);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    TextRecognized?.Invoke(this, text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in capture and recognize: {ex.Message}");
                RecognitionError?.Invoke(this, ex);
            }
        }

        private Bitmap? CaptureScreenArea(DrawingRectangle area)
        {
            try
            {
                var bitmap = new Bitmap(area.Width, area.Height);
                using var graphics = Graphics.FromImage(bitmap);
                
                graphics.CopyFromScreen(area.Left, area.Top, 0, 0, area.Size);
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error capturing screen area: {ex.Message}");
                return null;
            }
        }

        private async Task<string> RecognizeTextFromBitmapAsync(Bitmap bitmap, OCRParameters parameters)
        {
            try
            {
                // Apply image preprocessing based on parameters
                var processedBitmap = ApplyImagePreprocessing(bitmap, parameters);
                
                // Convert Bitmap to SoftwareBitmap
                using var stream = new MemoryStream();
                processedBitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                // Create OCR engine with specified language
                var ocrEngine = CreateOCREngine(parameters.Language);
                if (ocrEngine == null)
                {
                    throw new InvalidOperationException("No OCR engine available");
                }

                // Perform OCR
                var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
                
                var textBuilder = new StringBuilder();
                foreach (var line in ocrResult.Lines)
                {
                    textBuilder.AppendLine(line.Text);
                }

                var rawText = textBuilder.ToString().Trim();
                
                if (string.IsNullOrWhiteSpace(rawText))
                {
                    return string.Empty;
                }

                _logger.LogDebug($"🔍 OCR Raw Text: '{rawText}'");
                
                // Step 1: Clean up the OCR data
                var cleanedText = await _textCleaningService.FixOCRTextAsync(rawText);
                _logger.LogDebug($"🧹 Cleaned Text: '{cleanedText}'");
                
                // Step 2: Translate to target language (if different from source)
                var targetLanguage = GetCurrentTargetLanguage();
                if (!string.IsNullOrEmpty(targetLanguage) && targetLanguage != "en")
                {
                    try
                    {
                        var translatedText = await _translationService.TranslateAsync(cleanedText, "en", targetLanguage);
                        _logger.LogDebug($"🌐 Translated Text: '{translatedText}'");
                        
                        // Step 3: Improve the translation quality
                        var improvedText = await _textCleaningService.ImproveTranslationAsync(translatedText, targetLanguage);
                        _logger.LogDebug($"✨ Improved Translation: '{improvedText}'");
                        
                        return improvedText;
                    }
                    catch (Exception translationEx)
                    {
                        _logger.LogWarning($"⚠️ Translation failed, returning cleaned text: {translationEx.Message}");
                        return cleanedText; // Return cleaned text if translation fails
                    }
                }
                
                return cleanedText;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in OCR recognition: {ex.Message}");
                throw;
            }
        }

        private string GetCurrentTargetLanguage()
        {
            // This should get the current target language from the application settings
            // For now, we'll return a default or get it from the main window
            try
            {
                // Try to get the target language from the main window or settings
                if (Application.Current?.MainWindow is MainWindow_Working mainWindow)
                {
                    // You can add a property to MainWindow_Working to expose the current target language
                    // For now, we'll return a default
                    return "en"; // Default to English, you can change this
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not get target language: {ex.Message}");
            }
            
            return "en"; // Default fallback
        }
        
        private Bitmap ApplyImagePreprocessing(Bitmap originalBitmap, OCRParameters parameters)
        {
            try
            {
                // For now, return the original bitmap
                // In a full implementation, you would apply contrast, brightness, sharpness, etc.
                // based on the parameters
                _logger.LogDebug($"Applying image preprocessing: Contrast={parameters.Contrast}, Brightness={parameters.Brightness}, Sharpness={parameters.Sharpness}");
                
                // TODO: Implement actual image preprocessing
                // This would include:
                // - Contrast adjustment
                // - Brightness adjustment  
                // - Sharpness enhancement
                // - Noise reduction
                // - Deskewing
                // - Binarization
                
                return new Bitmap(originalBitmap);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Image preprocessing failed, using original: {ex.Message}");
                return new Bitmap(originalBitmap);
            }
        }
        
        private OcrEngine? CreateOCREngine(string language)
        {
            try
            {
                // Try to create engine with specified language
                var languages = OcrEngine.AvailableRecognizerLanguages;
                var targetLanguage = languages.FirstOrDefault(l => l.LanguageTag.StartsWith(language));
                
                if (targetLanguage != null)
                {
                    return OcrEngine.TryCreateFromLanguage(targetLanguage);
                }
                
                // Fallback to user profile languages
                return OcrEngine.TryCreateFromUserProfileLanguages();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to create OCR engine for language {language}: {ex.Message}");
                return OcrEngine.TryCreateFromUserProfileLanguages();
            }
        }
    }

    // Area selection window for capturing screen regions
    public class AreaSelectionWindow : Window
    {
        public event Action<DrawingRectangle>? AreaSelected;
        public event Action? SelectionCancelled;

        private readonly AreaSelectionOverlay _overlay;

        public AreaSelectionWindow()
        {
            Title = "Select Capture Area";
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;

            // Make window cover entire screen
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            _overlay = new AreaSelectionOverlay();
            Content = _overlay;

            _overlay.AreaSelected += OnAreaSelected;
            _overlay.Cancelled += OnCancelled;

            KeyDown += OnKeyDown;
            Focusable = true;
            Focus();
        }

        private void OnAreaSelected(DrawingRectangle area)
        {
            AreaSelected?.Invoke(area);
            Close();
        }

        private void OnCancelled()
        {
            SelectionCancelled?.Invoke();
            Close();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OnCancelled();
            }
        }
    }

    public class AreaSelectionOverlay : UserControl
    {
        public event Action<DrawingRectangle>? AreaSelected;
        public event Action? Cancelled;

        private System.Windows.Point _startPoint;
        private bool _isSelecting;
        private DrawingRectangle? _selectionRect;
        private Canvas _canvas;

        public AreaSelectionOverlay()
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 0));

            _canvas = new Canvas();
            Content = _canvas;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            KeyDown += OnKeyDown;
            Focusable = true;
            Focus();

            // Add instructions
            var instructions = new TextBlock
            {
                Text = "Click and drag to select an area. Press ESC to cancel.",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0)
            };
            _canvas.Children.Add(instructions);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(this);
            _isSelecting = true;
            CaptureMouse();

            _selectionRect = new DrawingRectangle();
            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting) return;

            var currentPoint = e.GetPosition(this);
            var left = Math.Min(_startPoint.X, currentPoint.X);
            var top = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(currentPoint.X - _startPoint.X);
            var height = Math.Abs(currentPoint.Y - _startPoint.Y);

            // Update visual feedback
            UpdateSelectionVisual(left, top, width, height);

            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting) return;

            _isSelecting = false;
            ReleaseMouseCapture();

            var currentPoint = e.GetPosition(this);
            var left = Math.Min(_startPoint.X, currentPoint.X);
            var top = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(currentPoint.X - _startPoint.X);
            var height = Math.Abs(currentPoint.Y - _startPoint.Y);

            if (width > 10 && height > 10)
            {
                var area = new DrawingRectangle((int)left, (int)top, (int)width, (int)height);
                AreaSelected?.Invoke(area);
            }

            e.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Cancelled?.Invoke();
            }
        }

        private void UpdateSelectionVisual(double left, double top, double width, double height)
        {
            // Clear previous selection visual
            _canvas.Children.Clear();

            // Add selection rectangle
            var rect = new WpfRectangle
            {
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 0, 0)),
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rect, left);
            Canvas.SetTop(rect, top);
            _canvas.Children.Add(rect);

            // Add instructions
            var instructions = new TextBlock
            {
                Text = "Click and drag to select an area. Press ESC to cancel.",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0)
            };
            _canvas.Children.Add(instructions);
        }
    }
}
