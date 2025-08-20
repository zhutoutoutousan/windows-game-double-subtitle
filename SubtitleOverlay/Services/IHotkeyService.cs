namespace SubtitleOverlay.Services
{
    public interface IHotkeyService
    {
        event EventHandler<string>? HotkeyPressed;
        void RegisterHotkey(string hotkey, string action);
        void UnregisterHotkey(string action);
        void Start();
        void Stop();
    }
}
