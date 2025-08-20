namespace SubtitleOverlay.Services
{
    public interface ISpeechRecognitionService
    {
        event EventHandler<string>? SpeechRecognized;
        event EventHandler<string>? SpeechHypothesis;
        event EventHandler<Exception>? RecognitionError;

        bool IsListening { get; }
        Task StartAsync();
        Task StopAsync();
        void SetConfidenceThreshold(double threshold);
    }
}
