namespace SubtitleOverlay.Models
{
    public class SubtitleSettings
    {
        public string TargetLanguage { get; set; } = "en";
        public string SourceLanguage { get; set; } = "auto";
        public int FontSize { get; set; } = 24;
        public string FontFamily { get; set; } = "Arial";
        public string FontColor { get; set; } = "#FFFFFF";
        public string BackgroundColor { get; set; } = "#000000";
        public double BackgroundOpacity { get; set; } = 0.7;
        public int MaxWidth { get; set; } = 800;
        public string Position { get; set; } = "Bottom";
        public bool ShowOriginalText { get; set; } = true;
        public bool ShowTranslatedText { get; set; } = true;
    }

    public class SpeechRecognitionSettings
    {
        public bool Enabled { get; set; } = true;
        public double ConfidenceThreshold { get; set; } = 0.7;
        public string GrammarFile { get; set; } = "";
        public bool UseDictationMode { get; set; } = true;
    }

    public class TranslationSettings
    {
        public string Provider { get; set; } = "Google";
        public string ApiKey { get; set; } = "";
        public string ApiEndpoint { get; set; } = "https://translation.googleapis.com/language/translate/v2";
        public bool CacheEnabled { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 60;
    }

    public class HotkeySettings
    {
        public string ToggleOverlay { get; set; } = "Ctrl+Shift+S";
        public string ToggleRecognition { get; set; } = "Ctrl+Shift+R";
        public string Exit { get; set; } = "Ctrl+Shift+Q";
    }

    public class OverlaySettings
    {
        public bool AlwaysOnTop { get; set; } = true;
        public bool ClickThrough { get; set; } = false;
        public bool Draggable { get; set; } = true;
        public bool Resizable { get; set; } = false;
        public Position StartPosition { get; set; } = new Position { X = 100, Y = 100 };
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
