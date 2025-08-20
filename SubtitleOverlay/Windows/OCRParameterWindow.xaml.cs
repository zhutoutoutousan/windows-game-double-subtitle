using System;
using System.Windows;
using System.Windows.Controls;
using SubtitleOverlay.Models;

namespace SubtitleOverlay.Windows
{
    public partial class OCRParameterWindow : Window
    {
        public OCRParameters Parameters { get; private set; }
        public bool IsTestRequested { get; private set; } = false;
        
        // Event to notify parent window about test results
        public event EventHandler<string>? TestCompleted;
        
        public OCRParameterWindow(OCRParameters? initialParameters = null)
        {
            InitializeComponent();
            
            // Initialize with provided parameters or defaults
            Parameters = initialParameters?.Clone() ?? OCRParameters.GetDefault();
            
            // Load parameters into UI controls after window is loaded
            this.Loaded += (s, e) => LoadParametersToUI();
        }
        
        private void LoadParametersToUI()
        {
            try
            {
                // Image preprocessing
                if (ContrastSlider != null) ContrastSlider.Value = Parameters.Contrast;
                if (BrightnessSlider != null) BrightnessSlider.Value = Parameters.Brightness;
                if (SharpnessSlider != null) SharpnessSlider.Value = Parameters.Sharpness;
                if (NoiseReductionCheckBox != null) NoiseReductionCheckBox.IsChecked = Parameters.EnableNoiseReduction;
                if (DeskewCheckBox != null) DeskewCheckBox.IsChecked = Parameters.EnableDeskew;
                
                // Text recognition
                if (ConfidenceSlider != null) ConfidenceSlider.Value = Parameters.MinimumConfidence;
                if (ScaleFactorSlider != null) ScaleFactorSlider.Value = Parameters.TextScaleFactor;
                if (MinHeightSlider != null) MinHeightSlider.Value = Parameters.MinimumTextHeight;
                if (MaxHeightSlider != null) MaxHeightSlider.Value = Parameters.MaximumTextHeight;
                if (WordSegmentationCheckBox != null) WordSegmentationCheckBox.IsChecked = Parameters.EnableWordSegmentation;
                if (LineSegmentationCheckBox != null) LineSegmentationCheckBox.IsChecked = Parameters.EnableLineSegmentation;
                
                // Language and character set
                SetLanguageInComboBox(Parameters.Language);
                if (NumberRecognitionCheckBox != null) NumberRecognitionCheckBox.IsChecked = Parameters.EnableNumberRecognition;
                if (SymbolRecognitionCheckBox != null) SymbolRecognitionCheckBox.IsChecked = Parameters.EnableSymbolRecognition;
                if (PunctuationRecognitionCheckBox != null) PunctuationRecognitionCheckBox.IsChecked = Parameters.EnablePunctuationRecognition;
                
                // Advanced settings
                if (BinarizationCheckBox != null) BinarizationCheckBox.IsChecked = Parameters.EnableBinarization;
                if (BinarizationThresholdSlider != null)
                {
                    BinarizationThresholdSlider.Value = Parameters.BinarizationThreshold;
                    BinarizationThresholdSlider.IsEnabled = Parameters.EnableBinarization;
                }
                if (MorphologicalOperationsCheckBox != null) MorphologicalOperationsCheckBox.IsChecked = Parameters.EnableMorphologicalOperations;
                
                // Description
                if (DescriptionTextBox != null) DescriptionTextBox.Text = Parameters.Description;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading parameters to UI: {ex.Message}");
            }
        }
        
        private void SetLanguageInComboBox(string language)
        {
            if (LanguageComboBox?.Items == null) return;
            
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == language)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        
        private void UpdateParametersFromUI()
        {
            try
            {
                // Image preprocessing
                if (ContrastSlider != null) Parameters.Contrast = ContrastSlider.Value;
                if (BrightnessSlider != null) Parameters.Brightness = BrightnessSlider.Value;
                if (SharpnessSlider != null) Parameters.Sharpness = SharpnessSlider.Value;
                if (NoiseReductionCheckBox != null) Parameters.EnableNoiseReduction = NoiseReductionCheckBox.IsChecked ?? true;
                if (DeskewCheckBox != null) Parameters.EnableDeskew = DeskewCheckBox.IsChecked ?? true;
                
                // Text recognition
                if (ConfidenceSlider != null) Parameters.MinimumConfidence = ConfidenceSlider.Value;
                if (ScaleFactorSlider != null) Parameters.TextScaleFactor = ScaleFactorSlider.Value;
                if (MinHeightSlider != null) Parameters.MinimumTextHeight = (int)MinHeightSlider.Value;
                if (MaxHeightSlider != null) Parameters.MaximumTextHeight = (int)MaxHeightSlider.Value;
                if (WordSegmentationCheckBox != null) Parameters.EnableWordSegmentation = WordSegmentationCheckBox.IsChecked ?? true;
                if (LineSegmentationCheckBox != null) Parameters.EnableLineSegmentation = LineSegmentationCheckBox.IsChecked ?? true;
                
                // Language and character set
                if (LanguageComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    Parameters.Language = selectedItem.Tag?.ToString() ?? "en-US";
                }
                if (NumberRecognitionCheckBox != null) Parameters.EnableNumberRecognition = NumberRecognitionCheckBox.IsChecked ?? true;
                if (SymbolRecognitionCheckBox != null) Parameters.EnableSymbolRecognition = SymbolRecognitionCheckBox.IsChecked ?? true;
                if (PunctuationRecognitionCheckBox != null) Parameters.EnablePunctuationRecognition = PunctuationRecognitionCheckBox.IsChecked ?? true;
                
                // Advanced settings
                if (BinarizationCheckBox != null) Parameters.EnableBinarization = BinarizationCheckBox.IsChecked ?? false;
                if (BinarizationThresholdSlider != null) Parameters.BinarizationThreshold = BinarizationThresholdSlider.Value;
                if (MorphologicalOperationsCheckBox != null) Parameters.EnableMorphologicalOperations = MorphologicalOperationsCheckBox.IsChecked ?? false;
                
                // Description
                if (DescriptionTextBox != null) Parameters.Description = DescriptionTextBox.Text;
                Parameters.LastModified = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating parameters from UI: {ex.Message}");
            }
        }
        
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (sender == ContrastSlider && ContrastValue != null)
                    ContrastValue.Text = ContrastSlider.Value.ToString("F1");
                else if (sender == BrightnessSlider && BrightnessValue != null)
                    BrightnessValue.Text = BrightnessSlider.Value.ToString("F1");
                else if (sender == SharpnessSlider && SharpnessValue != null)
                    SharpnessValue.Text = SharpnessSlider.Value.ToString("F1");
                else if (sender == ConfidenceSlider && ConfidenceValue != null)
                    ConfidenceValue.Text = ConfidenceSlider.Value.ToString("F1");
                else if (sender == ScaleFactorSlider && ScaleFactorValue != null)
                    ScaleFactorValue.Text = ScaleFactorSlider.Value.ToString("F1");
                else if (sender == MinHeightSlider && MinHeightValue != null)
                    MinHeightValue.Text = MinHeightSlider.Value.ToString("F0");
                else if (sender == MaxHeightSlider && MaxHeightValue != null)
                    MaxHeightValue.Text = MaxHeightSlider.Value.ToString("F0");
                else if (sender == BinarizationThresholdSlider && BinarizationThresholdValue != null)
                    BinarizationThresholdValue.Text = BinarizationThresholdSlider.Value.ToString("F1");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in slider value changed: {ex.Message}");
            }
        }
        
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Language selection changed - could add validation here
        }
        
        private void BinarizationCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (BinarizationThresholdSlider != null && BinarizationCheckBox != null)
            {
                BinarizationThresholdSlider.IsEnabled = BinarizationCheckBox.IsChecked ?? false;
            }
        }
        
        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            Parameters = OCRParameters.GetDefault();
            LoadParametersToUI();
        }
        
        private void SmallTextButton_Click(object sender, RoutedEventArgs e)
        {
            Parameters = OCRParameters.GetSmallTextOptimized();
            LoadParametersToUI();
        }
        
        private void LargeTextButton_Click(object sender, RoutedEventArgs e)
        {
            Parameters = OCRParameters.GetLargeTextOptimized();
            LoadParametersToUI();
        }
        
        private void LowContrastButton_Click(object sender, RoutedEventArgs e)
        {
            Parameters = OCRParameters.GetLowContrastOptimized();
            LoadParametersToUI();
        }
        
        private void QuickTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update parameters from UI
                UpdateParametersFromUI();
                
                // Show test results section
                TestResultsGroupBox.Visibility = Visibility.Visible;
                TestStatusText.Text = "Testing OCR parameters...";
                TestResultTextBox.Text = "";
                
                // Disable the test button during testing
                QuickTestButton.IsEnabled = false;
                
                // Perform the test (this will be handled by the parent window)
                TestCompleted?.Invoke(this, "quick_test");
                
                // The actual test result will be set by the parent window calling SetTestResult
            }
            catch (Exception ex)
            {
                TestStatusText.Text = $"Error: {ex.Message}";
                TestResultTextBox.Text = "Test failed due to an error.";
            }
            finally
            {
                QuickTestButton.IsEnabled = true;
            }
        }
        
        public void SetTestResult(string result, bool isSuccess)
        {
            TestResultTextBox.Text = result;
            TestStatusText.Text = isSuccess ? "Test completed successfully!" : "Test completed but no text was recognized.";
            TestStatusText.Foreground = isSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Orange;
        }
        
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateParametersFromUI();
            IsTestRequested = true;
            DialogResult = true;
            Close();
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateParametersFromUI();
            IsTestRequested = false;
            DialogResult = true;
            Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
