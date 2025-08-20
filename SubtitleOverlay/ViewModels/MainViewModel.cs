using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitleOverlay.Models;
using SubtitleOverlay.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SubtitleOverlay.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<MainViewModel> _logger;
        private readonly ISpeechRecognitionService _speechRecognitionService;
        private readonly ITranslationService _translationService;
        private readonly IOverlayService _overlayService;
        private readonly IHotkeyService _hotkeyService;
        private readonly SubtitleSettings _subtitleSettings;

        private bool _isListening;
        private bool _isOverlayVisible;
        private string _selectedTargetLanguage = "en";
        private int _fontSize = 24;
        private bool _showOriginalText = true;
        private bool _showTranslatedText = true;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            ISpeechRecognitionService speechRecognitionService,
            ITranslationService translationService,
            IOverlayService overlayService,
            IHotkeyService hotkeyService,
            IOptions<SubtitleSettings> subtitleSettings)
        {
            _logger = logger;
            _speechRecognitionService = speechRecognitionService;
            _translationService = translationService;
            _overlayService = overlayService;
            _hotkeyService = hotkeyService;
            _subtitleSettings = subtitleSettings.Value;

            // Initialize commands
            StartRecognitionCommand = new RelayCommand(StartRecognition, CanStartRecognition);
            StopRecognitionCommand = new RelayCommand(StopRecognition, CanStopRecognition);
            ShowOverlayCommand = new RelayCommand(ShowOverlay, CanShowOverlay);
            HideOverlayCommand = new RelayCommand(HideOverlay, CanHideOverlay);
            SaveApiKeyCommand = new RelayCommand<string>(SaveApiKey);
            TestConnectionCommand = new RelayCommand(TestConnection, CanTestConnection);

            // Initialize available languages
            AvailableLanguages = new ObservableCollection<string>
            {
                "en", "es", "fr", "de", "it", "pt", "ru", "ja", "ko", "zh", "ar", "hi", "nl", "sv", "da", "no", "fi", "pl", "tr", "th"
            };

            // Subscribe to events
            _speechRecognitionService.SpeechRecognized += OnSpeechRecognized;
            _speechRecognitionService.RecognitionError += OnRecognitionError;
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            // Start hotkey service
            _hotkeyService.Start();

            // Initialize properties
            SelectedTargetLanguage = _subtitleSettings.TargetLanguage;
            FontSize = _subtitleSettings.FontSize;
            ShowOriginalText = _subtitleSettings.ShowOriginalText;
            ShowTranslatedText = _subtitleSettings.ShowTranslatedText;
        }

        public bool IsListening
        {
            get => _isListening;
            set => SetProperty(ref _isListening, value);
        }

        public bool IsTranslationAvailable => _translationService.IsAvailable;

        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        public string SelectedTargetLanguage
        {
            get => _selectedTargetLanguage;
            set => SetProperty(ref _selectedTargetLanguage, value);
        }

        public int FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public bool ShowOriginalText
        {
            get => _showOriginalText;
            set => SetProperty(ref _showOriginalText, value);
        }

        public bool ShowTranslatedText
        {
            get => _showTranslatedText;
            set => SetProperty(ref _showTranslatedText, value);
        }

        public ObservableCollection<string> AvailableLanguages { get; }

        public ICommand StartRecognitionCommand { get; }
        public ICommand StopRecognitionCommand { get; }
        public ICommand ShowOverlayCommand { get; }
        public ICommand HideOverlayCommand { get; }
        public ICommand SaveApiKeyCommand { get; }
        public ICommand TestConnectionCommand { get; }

        private async void StartRecognition()
        {
            try
            {
                await _speechRecognitionService.StartAsync();
                IsListening = true;
                _logger.LogInformation("Speech recognition started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start speech recognition");
            }
        }

        private bool CanStartRecognition() => !IsListening;

        private async void StopRecognition()
        {
            try
            {
                await _speechRecognitionService.StopAsync();
                IsListening = false;
                _logger.LogInformation("Speech recognition stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop speech recognition");
            }
        }

        private bool CanStopRecognition() => IsListening;

        private void ShowOverlay()
        {
            _overlayService.Show();
            IsOverlayVisible = true;
            _logger.LogInformation("Overlay shown");
        }

        private bool CanShowOverlay() => !IsOverlayVisible;

        private void HideOverlay()
        {
            _overlayService.Hide();
            IsOverlayVisible = false;
            _logger.LogInformation("Overlay hidden");
        }

        private bool CanHideOverlay() => IsOverlayVisible;

        private void SaveApiKey(string? apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                // In a real application, you'd save this securely
                _logger.LogInformation("API key saved");
            }
        }

        private async void TestConnection()
        {
            try
            {
                var testText = "Hello world";
                var translated = await _translationService.TranslateAsync(testText, SelectedTargetLanguage);
                _logger.LogInformation("Translation test successful: {Original} -> {Translated}", testText, translated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Translation test failed");
            }
        }

        private bool CanTestConnection() => IsTranslationAvailable;

        private async void OnSpeechRecognized(object? sender, string recognizedText)
        {
            _logger.LogInformation("Speech recognized: {Text}", recognizedText);

            string? translatedText = null;
            if (IsTranslationAvailable && ShowTranslatedText)
            {
                try
                {
                    translatedText = await _translationService.TranslateAsync(recognizedText, SelectedTargetLanguage);
                    _logger.LogInformation("Translated: {Original} -> {Translated}", recognizedText, translatedText);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Translation failed for: {Text}", recognizedText);
                }
            }

            _overlayService.UpdateSubtitle(
                ShowOriginalText ? recognizedText : "",
                ShowTranslatedText ? translatedText : recognizedText
            );
        }

        private void OnRecognitionError(object? sender, Exception ex)
        {
            _logger.LogError(ex, "Speech recognition error");
        }

        private void OnHotkeyPressed(object? sender, string action)
        {
            switch (action)
            {
                case "ToggleOverlay":
                    if (IsOverlayVisible)
                        HideOverlay();
                    else
                        ShowOverlay();
                    break;
                case "ToggleRecognition":
                    if (IsListening)
                        StopRecognition();
                    else
                        StartRecognition();
                    break;
                case "Exit":
                    Application.Current.Shutdown();
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}
