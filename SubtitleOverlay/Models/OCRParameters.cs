using System;

namespace SubtitleOverlay.Models
{
    public class OCRParameters
    {
        // Image preprocessing parameters
        public double Contrast { get; set; } = 1.0; // 0.5 to 2.0
        public double Brightness { get; set; } = 0.0; // -1.0 to 1.0
        public double Sharpness { get; set; } = 1.0; // 0.5 to 2.0
        public bool EnableNoiseReduction { get; set; } = true;
        public bool EnableDeskew { get; set; } = true;
        
        // Text recognition parameters
        public double MinimumConfidence { get; set; } = 0.6; // 0.0 to 1.0
        public bool EnableWordSegmentation { get; set; } = true;
        public bool EnableLineSegmentation { get; set; } = true;
        public int MinimumTextHeight { get; set; } = 8; // pixels
        public int MaximumTextHeight { get; set; } = 100; // pixels
        
        // Language and character set parameters
        public string Language { get; set; } = "en-US";
        public bool EnableNumberRecognition { get; set; } = true;
        public bool EnableSymbolRecognition { get; set; } = true;
        public bool EnablePunctuationRecognition { get; set; } = true;
        
        // Advanced parameters
        public double TextScaleFactor { get; set; } = 1.0; // 0.5 to 3.0
        public bool EnableBinarization { get; set; } = false;
        public double BinarizationThreshold { get; set; } = 0.5; // 0.0 to 1.0
        public bool EnableMorphologicalOperations { get; set; } = false;
        
        // Area-specific parameters
        public string AreaId { get; set; } = "";
        public DateTime LastModified { get; set; } = DateTime.Now;
        public string Description { get; set; } = "";
        
        // Create a copy of current parameters
        public OCRParameters Clone()
        {
            return new OCRParameters
            {
                Contrast = this.Contrast,
                Brightness = this.Brightness,
                Sharpness = this.Sharpness,
                EnableNoiseReduction = this.EnableNoiseReduction,
                EnableDeskew = this.EnableDeskew,
                MinimumConfidence = this.MinimumConfidence,
                EnableWordSegmentation = this.EnableWordSegmentation,
                EnableLineSegmentation = this.EnableLineSegmentation,
                MinimumTextHeight = this.MinimumTextHeight,
                MaximumTextHeight = this.MaximumTextHeight,
                Language = this.Language,
                EnableNumberRecognition = this.EnableNumberRecognition,
                EnableSymbolRecognition = this.EnableSymbolRecognition,
                EnablePunctuationRecognition = this.EnablePunctuationRecognition,
                TextScaleFactor = this.TextScaleFactor,
                EnableBinarization = this.EnableBinarization,
                BinarizationThreshold = this.BinarizationThreshold,
                EnableMorphologicalOperations = this.EnableMorphologicalOperations,
                AreaId = this.AreaId,
                LastModified = this.LastModified,
                Description = this.Description
            };
        }
        
        // Get default parameters
        public static OCRParameters GetDefault()
        {
            return new OCRParameters();
        }
        
        // Get parameters optimized for small text
        public static OCRParameters GetSmallTextOptimized()
        {
            return new OCRParameters
            {
                Contrast = 1.2,
                Brightness = 0.1,
                Sharpness = 1.3,
                EnableNoiseReduction = true,
                EnableDeskew = true,
                MinimumConfidence = 0.5,
                EnableWordSegmentation = true,
                EnableLineSegmentation = true,
                MinimumTextHeight = 6,
                MaximumTextHeight = 50,
                Language = "en-US",
                EnableNumberRecognition = true,
                EnableSymbolRecognition = true,
                EnablePunctuationRecognition = true,
                TextScaleFactor = 1.5,
                EnableBinarization = false,
                BinarizationThreshold = 0.5,
                EnableMorphologicalOperations = false
            };
        }
        
        // Get parameters optimized for large text
        public static OCRParameters GetLargeTextOptimized()
        {
            return new OCRParameters
            {
                Contrast = 1.0,
                Brightness = 0.0,
                Sharpness = 1.0,
                EnableNoiseReduction = true,
                EnableDeskew = true,
                MinimumConfidence = 0.7,
                EnableWordSegmentation = true,
                EnableLineSegmentation = true,
                MinimumTextHeight = 12,
                MaximumTextHeight = 200,
                Language = "en-US",
                EnableNumberRecognition = true,
                EnableSymbolRecognition = true,
                EnablePunctuationRecognition = true,
                TextScaleFactor = 1.0,
                EnableBinarization = false,
                BinarizationThreshold = 0.5,
                EnableMorphologicalOperations = false
            };
        }
        
        // Get parameters optimized for low contrast text
        public static OCRParameters GetLowContrastOptimized()
        {
            return new OCRParameters
            {
                Contrast = 1.5,
                Brightness = 0.2,
                Sharpness = 1.2,
                EnableNoiseReduction = true,
                EnableDeskew = true,
                MinimumConfidence = 0.4,
                EnableWordSegmentation = true,
                EnableLineSegmentation = true,
                MinimumTextHeight = 8,
                MaximumTextHeight = 100,
                Language = "en-US",
                EnableNumberRecognition = true,
                EnableSymbolRecognition = true,
                EnablePunctuationRecognition = true,
                TextScaleFactor = 1.3,
                EnableBinarization = true,
                BinarizationThreshold = 0.4,
                EnableMorphologicalOperations = true
            };
        }
    }
}
