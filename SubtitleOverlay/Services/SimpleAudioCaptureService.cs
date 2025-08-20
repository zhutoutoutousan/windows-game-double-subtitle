using System;
using System.Threading;
using System.Threading.Tasks;

namespace SubtitleOverlay.Services
{
    public class SimpleAudioCaptureService
    {
        private bool _isCapturing = false;
        private Thread? _captureThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private Random _random = new Random();

        public event EventHandler<byte[]>? AudioDataReceived;
        public event EventHandler<string>? AudioLevelChanged;
        public event EventHandler<string>? SpeechDetected;

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

                    _isCapturing = true;
                    _cancellationTokenSource = new CancellationTokenSource();

                    // Start simulation thread
                    _captureThread = new Thread(() => SimulateAudioCapture(_cancellationTokenSource.Token));
                    _captureThread.Start();

                    System.Diagnostics.Debug.WriteLine("Simple audio capture started successfully");
                    return true;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Simple audio capture start failed: {ex.Message}");
                return false;
            }
        }

        public async Task StopCaptureAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Stopping simple audio capture...");

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

                    System.Diagnostics.Debug.WriteLine("Simple audio capture stopped successfully");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Simple audio capture stop failed: {ex.Message}");
            }
        }

        private void SimulateAudioCapture(CancellationToken cancellationToken)
        {
            int frameCount = 0;
            
            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                try
                {
                    Thread.Sleep(100); // 10 FPS simulation
                    frameCount++;

                    // Generate simulated audio data
                    var audioData = new byte[4096];
                    _random.NextBytes(audioData);

                    // Simulate varying audio levels
                    float audioLevel = 0.0f;
                    
                    // Create some realistic audio patterns
                    if (frameCount % 50 < 10) // Every 5 seconds, simulate some activity
                    {
                        audioLevel = 0.3f + (float)(_random.NextDouble() * 0.4f); // 0.3 to 0.7
                        
                        // Add some variation to make it look more realistic
                        for (int i = 0; i < audioData.Length; i += 2)
                        {
                            if (i + 1 < audioData.Length)
                            {
                                short sample = (short)(_random.Next(-1000, 1000));
                                audioData[i] = (byte)(sample & 0xFF);
                                audioData[i + 1] = (byte)((sample >> 8) & 0xFF);
                            }
                        }
                    }
                    else
                    {
                        audioLevel = 0.01f + (float)(_random.NextDouble() * 0.05f); // 0.01 to 0.06 (background)
                    }

                    // Send audio data
                    AudioDataReceived?.Invoke(this, audioData);
                    AudioLevelChanged?.Invoke(this, $"Audio Level: {audioLevel:F3}");

                    // Detect "speech" based on audio level
                    if (audioLevel > 0.2f)
                    {
                        SpeechDetected?.Invoke(this, $"Speech detected! Level: {audioLevel:F3}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Audio simulation error: {ex.Message}");
                }
            }
        }

        public bool IsCapturing => _isCapturing;
    }
}
