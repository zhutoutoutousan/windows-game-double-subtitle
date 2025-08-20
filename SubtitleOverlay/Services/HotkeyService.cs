using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitleOverlay.Models;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace SubtitleOverlay.Services
{
    public class HotkeyService : IHotkeyService, IDisposable
    {
        private readonly ILogger<HotkeyService> _logger;
        private readonly HotkeySettings _settings;
        private readonly Dictionary<string, int> _registeredHotkeys;
        private readonly Dictionary<int, string> _hotkeyActions;
        private int _nextHotkeyId = 1;
        private bool _disposed;

        public event EventHandler<string>? HotkeyPressed;

        public HotkeyService(ILogger<HotkeyService> logger, IOptions<HotkeySettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _registeredHotkeys = new Dictionary<string, int>();
            _hotkeyActions = new Dictionary<int, string>();
        }

        public void RegisterHotkey(string hotkey, string action)
        {
            try
            {
                var (modifiers, key) = ParseHotkey(hotkey);
                var hotkeyId = _nextHotkeyId++;

                if (RegisterHotKey(IntPtr.Zero, hotkeyId, modifiers, key))
                {
                    _registeredHotkeys[action] = hotkeyId;
                    _hotkeyActions[hotkeyId] = action;
                    _logger.LogInformation("Registered hotkey {Hotkey} for action {Action}", hotkey, action);
                }
                else
                {
                    _logger.LogError("Failed to register hotkey {Hotkey} for action {Action}", hotkey, action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering hotkey {Hotkey} for action {Action}", hotkey, action);
            }
        }

        public void UnregisterHotkey(string action)
        {
            if (_registeredHotkeys.TryGetValue(action, out var hotkeyId))
            {
                if (UnregisterHotKey(IntPtr.Zero, hotkeyId))
                {
                    _registeredHotkeys.Remove(action);
                    _hotkeyActions.Remove(hotkeyId);
                    _logger.LogInformation("Unregistered hotkey for action {Action}", action);
                }
            }
        }

        public void Start()
        {
            // Register default hotkeys
            RegisterHotkey(_settings.ToggleOverlay, "ToggleOverlay");
            RegisterHotkey(_settings.ToggleRecognition, "ToggleRecognition");
            RegisterHotkey(_settings.Exit, "Exit");

            _logger.LogInformation("Hotkey service started");
        }

        public void Stop()
        {
            foreach (var action in _registeredHotkeys.Keys.ToList())
            {
                UnregisterHotkey(action);
            }
            _logger.LogInformation("Hotkey service stopped");
        }

        public void ProcessHotkeys()
        {
            var msg = new MSG();
            while (PeekMessage(ref msg, IntPtr.Zero, WM_HOTKEY, WM_HOTKEY, PM_REMOVE))
            {
                if (msg.message == WM_HOTKEY && _hotkeyActions.TryGetValue((int)msg.wParam, out var action))
                {
                    _logger.LogDebug("Hotkey pressed for action: {Action}", action);
                    HotkeyPressed?.Invoke(this, action);
                }
            }
        }

        private (uint modifiers, uint key) ParseHotkey(string hotkey)
        {
            var parts = hotkey.Split('+');
            uint modifiers = 0;
            uint key = 0;

            foreach (var part in parts)
            {
                var trimmedPart = part.Trim().ToLower();
                switch (trimmedPart)
                {
                    case "ctrl":
                        modifiers |= MOD_CONTROL;
                        break;
                    case "alt":
                        modifiers |= MOD_ALT;
                        break;
                    case "shift":
                        modifiers |= MOD_SHIFT;
                        break;
                    case "win":
                        modifiers |= MOD_WIN;
                        break;
                    default:
                        key = GetVirtualKeyCode(trimmedPart);
                        break;
                }
            }

            return (modifiers, key);
        }

        private uint GetVirtualKeyCode(string key)
        {
            return key.ToUpper() switch
            {
                "A" => 0x41,
                "B" => 0x42,
                "C" => 0x43,
                "D" => 0x44,
                "E" => 0x45,
                "F" => 0x46,
                "G" => 0x47,
                "H" => 0x48,
                "I" => 0x49,
                "J" => 0x4A,
                "K" => 0x4B,
                "L" => 0x4C,
                "M" => 0x4D,
                "N" => 0x4E,
                "O" => 0x4F,
                "P" => 0x50,
                "Q" => 0x51,
                "R" => 0x52,
                "S" => 0x53,
                "T" => 0x54,
                "U" => 0x55,
                "V" => 0x56,
                "W" => 0x57,
                "X" => 0x58,
                "Y" => 0x59,
                "Z" => 0x5A,
                "0" => 0x30,
                "1" => 0x31,
                "2" => 0x32,
                "3" => 0x33,
                "4" => 0x34,
                "5" => 0x35,
                "6" => 0x36,
                "7" => 0x37,
                "8" => 0x38,
                "9" => 0x39,
                "F1" => 0x70,
                "F2" => 0x71,
                "F3" => 0x72,
                "F4" => 0x73,
                "F5" => 0x74,
                "F6" => 0x75,
                "F7" => 0x76,
                "F8" => 0x77,
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                _ => 0
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                Stop();
                _disposed = true;
            }
        }

        // Windows API declarations
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint WM_HOTKEY = 0x0312;
        private const uint PM_REMOVE = 0x0001;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
    }
}
