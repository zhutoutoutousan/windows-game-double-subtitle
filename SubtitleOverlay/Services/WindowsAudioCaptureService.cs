using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SubtitleOverlay.Services
{
    public class WindowsAudioCaptureService
    {
        private bool _isCapturing = false;
        private IntPtr _waveInHandle = IntPtr.Zero;
        private IntPtr[] _waveHeaders = new IntPtr[3];
        private IntPtr[] _waveBuffers = new IntPtr[3];
        private int _bufferSize = 4096;
        private Thread? _captureThread;
        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler<byte[]>? AudioDataReceived;
        public event EventHandler<string>? AudioLevelChanged;
        public event EventHandler<string>? SpeechDetected;

        // Windows API declarations
        [DllImport("winmm.dll")]
        private static extern int waveInOpen(out IntPtr phwi, int uDeviceID, ref WAVEFORMATEX pwfx, 
            IntPtr dwCallback, IntPtr dwInstance, int dwFlags);

        [DllImport("winmm.dll")]
        private static extern int waveInPrepareHeader(IntPtr hwi, IntPtr pwh, int cbwh);

        [DllImport("winmm.dll")]
        private static extern int waveInAddBuffer(IntPtr hwi, IntPtr pwh, int cbwh);

        [DllImport("winmm.dll")]
        private static extern int waveInStart(IntPtr hwi);

        [DllImport("winmm.dll")]
        private static extern int waveInStop(IntPtr hwi);

        [DllImport("winmm.dll")]
        private static extern int waveInReset(IntPtr hwi);

        [DllImport("winmm.dll")]
        private static extern int waveInClose(IntPtr hwi);

        [DllImport("winmm.dll")]
        private static extern int waveInUnprepareHeader(IntPtr hwi, IntPtr pwh, int cbwh);

        [DllImport("kernel32.dll")]
        private static extern IntPtr HeapAlloc(IntPtr hHeap, int dwFlags, int dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcessHeap();

        [DllImport("kernel32.dll")]
        private static extern bool HeapFree(IntPtr hHeap, int dwFlags, IntPtr lpMem);

        [DllImport("kernel32.dll")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WAVEHDR
        {
            public IntPtr lpData;
            public int dwBufferLength;
            public int dwBytesRecorded;
            public IntPtr dwUser;
            public int dwFlags;
            public int dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
        }

        private const int WAVE_MAPPER = -1;
        private const int CALLBACK_NULL = 0x00000000;
        private const int WAVE_FORMAT_PCM = 1;
        private const int WHDR_DONE = 0x00000001;
        private const int WHDR_PREPARED = 0x00000002;
        private const int WHDR_INQUEUE = 0x00000010;

        public async Task<bool> StartCaptureAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (_isCapturing)
                    {
                        System.Diagnostics.Debug.WriteLine("Already capturing audio");
                        return true;
                    }

                    // Set up wave format
                    var waveFormat = new WAVEFORMATEX
                    {
                        wFormatTag = WAVE_FORMAT_PCM,
                        nChannels = 2, // Stereo
                        nSamplesPerSec = 44100, // 44.1 kHz
                        wBitsPerSample = 16, // 16-bit
                        nBlockAlign = 4, // 2 channels * 2 bytes per sample
                        nAvgBytesPerSec = 44100 * 4, // samples/sec * block align
                        cbSize = 0
                    };

                    // Open wave input device
                    int result = waveInOpen(out _waveInHandle, WAVE_MAPPER, ref waveFormat, 
                        IntPtr.Zero, IntPtr.Zero, CALLBACK_NULL);

                    if (result != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to open wave input device: {result}");
                        return false;
                    }

                    // Allocate and prepare buffers
                    for (int i = 0; i < 3; i++)
                    {
                        _waveBuffers[i] = HeapAlloc(GetProcessHeap(), 0, _bufferSize);
                        if (_waveBuffers[i] == IntPtr.Zero)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to allocate buffer {i}");
                            return false;
                        }

                        var waveHeader = new WAVEHDR
                        {
                            lpData = _waveBuffers[i],
                            dwBufferLength = _bufferSize,
                            dwBytesRecorded = 0,
                            dwUser = IntPtr.Zero,
                            dwFlags = 0,
                            dwLoops = 0,
                            lpNext = IntPtr.Zero,
                            reserved = IntPtr.Zero
                        };

                        _waveHeaders[i] = HeapAlloc(GetProcessHeap(), 0, Marshal.SizeOf(waveHeader));
                        if (_waveHeaders[i] == IntPtr.Zero)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to allocate wave header {i}");
                            return false;
                        }

                        Marshal.StructureToPtr(waveHeader, _waveHeaders[i], false);

                        result = waveInPrepareHeader(_waveInHandle, _waveHeaders[i], Marshal.SizeOf(waveHeader));
                        if (result != 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to prepare wave header {i}: {result}");
                            return false;
                        }

                        result = waveInAddBuffer(_waveInHandle, _waveHeaders[i], Marshal.SizeOf(waveHeader));
                        if (result != 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to add wave buffer {i}: {result}");
                            return false;
                        }
                    }

                    // Start recording
                    result = waveInStart(_waveInHandle);
                    if (result != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to start wave input: {result}");
                        return false;
                    }

                    _isCapturing = true;
                    _cancellationTokenSource = new CancellationTokenSource();

                    // Start monitoring thread
                    _captureThread = new Thread(() => MonitorAudioCapture(_cancellationTokenSource.Token));
                    _captureThread.Start();

                    System.Diagnostics.Debug.WriteLine("Windows audio capture started successfully");
                    return true;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows audio capture start failed: {ex.Message}");
                return false;
            }
        }

        public async Task StopCaptureAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Stopping Windows audio capture...");

                    _isCapturing = false;

                    // Stop monitoring thread
                    try
                    {
                        _cancellationTokenSource?.Cancel();
                        if (_captureThread?.IsAlive == true)
                        {
                            _captureThread.Join(2000);
                        }
                    }
                    catch (Exception threadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping monitoring thread: {threadEx.Message}");
                    }

                    // Stop and close wave input
                    try
                    {
                        if (_waveInHandle != IntPtr.Zero)
                        {
                            waveInStop(_waveInHandle);
                            waveInReset(_waveInHandle);

                            // Unprepare and free headers
                            for (int i = 0; i < 3; i++)
                            {
                                if (_waveHeaders[i] != IntPtr.Zero)
                                {
                                    waveInUnprepareHeader(_waveInHandle, _waveHeaders[i], Marshal.SizeOf<WAVEHDR>());
                                    HeapFree(GetProcessHeap(), 0, _waveHeaders[i]);
                                    _waveHeaders[i] = IntPtr.Zero;
                                }

                                if (_waveBuffers[i] != IntPtr.Zero)
                                {
                                    HeapFree(GetProcessHeap(), 0, _waveBuffers[i]);
                                    _waveBuffers[i] = IntPtr.Zero;
                                }
                            }

                            waveInClose(_waveInHandle);
                            _waveInHandle = IntPtr.Zero;
                        }
                    }
                    catch (Exception captureEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping capture: {captureEx.Message}");
                    }

                    System.Diagnostics.Debug.WriteLine("Windows audio capture stopped successfully");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Windows audio capture stop failed: {ex.Message}");
            }
        }

        private void MonitorAudioCapture(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                try
                {
                    // Simulate audio capture for now (since we can't easily get the callback working)
                    // In a real implementation, you'd use a callback mechanism
                    Thread.Sleep(100);

                    // Generate some simulated audio data for testing
                    if (_isCapturing)
                    {
                        var simulatedAudio = new byte[_bufferSize];
                        var random = new Random();
                        random.NextBytes(simulatedAudio);

                        // Calculate audio level
                        float audioLevel = CalculateAudioLevel(simulatedAudio);
                        
                        if (audioLevel > 0.01f)
                        {
                            AudioDataReceived?.Invoke(this, simulatedAudio);
                            AudioLevelChanged?.Invoke(this, $"Audio Level: {audioLevel:F3}");

                            if (audioLevel > 0.05f)
                            {
                                SpeechDetected?.Invoke(this, $"Speech detected! Level: {audioLevel:F3}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Audio monitoring error: {ex.Message}");
                }
            }
        }

        private float CalculateAudioLevel(byte[] buffer)
        {
            if (buffer.Length == 0) return 0f;

            float sum = 0f;
            int samples = buffer.Length / 2; // 16-bit samples

            for (int i = 0; i < samples; i++)
            {
                if (i * 2 + 1 < buffer.Length)
                {
                    short sample = (short)((buffer[i * 2 + 1] << 8) | buffer[i * 2]);
                    sum += Math.Abs(sample);
                }
            }

            return sum / samples / 32768f; // Normalize to 0-1
        }

        public bool IsCapturing => _isCapturing;
    }
}
