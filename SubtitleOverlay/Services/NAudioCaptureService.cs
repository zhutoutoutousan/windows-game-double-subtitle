using System;
using System.Threading.Tasks;
using System.Threading;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace SubtitleOverlay.Services
{
    public class NAudioCaptureService
    {
        private bool _isCapturing = false;
        private WasapiLoopbackCapture? _capture;
        private WaveFileWriter? _writer;
        private Thread? _captureThread;
        private CancellationTokenSource? _cancellationTokenSource;
        private float _lastAudioLevel = 0f;
        private DateTime _lastAudioTime = DateTime.MinValue;

        public event EventHandler<byte[]>? AudioDataReceived;
        public event EventHandler<string>? AudioLevelChanged;
        public event EventHandler<string>? SpeechDetected;

        public async Task<bool> StartCaptureAsync()
        {
            try
            {
                // Use a timeout to prevent hanging
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                return await Task.Run(() =>
                {
                    try
                    {
                        // Check if we're already capturing
                        if (_isCapturing)
                        {
                            System.Diagnostics.Debug.WriteLine("Already capturing audio");
                            return true;
                        }

                        // Get default audio render device (what's playing audio)
                        MMDevice? device = null;
                        try
                        {
                            var enumerator = new MMDeviceEnumerator();
                            device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                            
                            if (device == null)
                            {
                                System.Diagnostics.Debug.WriteLine("No default audio render device found");
                                return false;
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Using audio device: {device.FriendlyName}");
                        }
                        catch (Exception deviceEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to get audio device: {deviceEx.Message}");
                            return false;
                        }
                        
                        // Create loopback capture (captures what's being played)
                        try
                        {
                            _capture = new WasapiLoopbackCapture(device);
                            
                            // Set up the data available event
                            _capture.DataAvailable += Capture_DataAvailable;
                            _capture.RecordingStopped += Capture_RecordingStopped;
                            
                            // Start capturing
                            _capture.StartRecording();
                            
                            _isCapturing = true;
                            _cancellationTokenSource = new CancellationTokenSource();
                            
                            // Start monitoring thread
                            _captureThread = new Thread(() => MonitorAudioLevel(_cancellationTokenSource.Token));
                            _captureThread.Start();
                            
                            System.Diagnostics.Debug.WriteLine("Audio capture started successfully");
                            return true;
                        }
                        catch (Exception captureEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to start audio capture: {captureEx.Message}");
                            _capture?.Dispose();
                            _capture = null;
                            return false;
                        }
                    }
                    catch (Exception innerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner NAudio capture start failed: {innerEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {innerEx.StackTrace}");
                        return false;
                    }
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Audio capture start timed out");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NAudio capture start failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await StopCaptureAsync();
                return false;
            }
        }

        public async Task StopCaptureAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Stopping audio capture...");
                    
                    _isCapturing = false;
                    
                    // Stop monitoring thread
                    try
                    {
                        _cancellationTokenSource?.Cancel();
                        if (_captureThread?.IsAlive == true)
                        {
                            _captureThread.Join(2000); // Wait up to 2 seconds
                        }
                    }
                    catch (Exception threadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping monitoring thread: {threadEx.Message}");
                    }
                    
                    // Stop capture
                    try
                    {
                        if (_capture != null)
                        {
                            _capture.StopRecording();
                            _capture.Dispose();
                            _capture = null;
                        }
                    }
                    catch (Exception captureEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping capture: {captureEx.Message}");
                    }
                    
                    // Dispose writer
                    try
                    {
                        _writer?.Dispose();
                        _writer = null;
                    }
                    catch (Exception writerEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing writer: {writerEx.Message}");
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Audio capture stopped successfully");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NAudio capture stop failed: {ex.Message}");
            }
        }

        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (!_isCapturing) return;
            
            try
            {
                // Calculate audio level
                float audioLevel = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
                _lastAudioLevel = audioLevel;
                _lastAudioTime = DateTime.Now;
                
                // Only process if there's significant audio
                if (audioLevel > 0.01f)
                {
                    // Copy audio data
                    byte[] audioData = new byte[e.BytesRecorded];
                    Array.Copy(e.Buffer, audioData, e.BytesRecorded);
                    
                    // Send audio data for speech recognition
                    AudioDataReceived?.Invoke(this, audioData);
                    AudioLevelChanged?.Invoke(this, $"Audio Level: {audioLevel:F3}");
                    
                    // Detect speech (simple threshold-based detection)
                    if (audioLevel > 0.05f)
                    {
                        SpeechDetected?.Invoke(this, $"Speech detected! Level: {audioLevel:F3}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio data processing error: {ex.Message}");
            }
        }

        private void Capture_RecordingStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                System.Diagnostics.Debug.WriteLine($"Recording stopped with error: {e.Exception.Message}");
            }
        }

        private void MonitorAudioLevel(CancellationToken cancellationToken)
        {
            bool wasSpeaking = false;
            
            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                try
                {
                    bool isCurrentlySpeaking = _lastAudioLevel > 0.05f;
                    
                    // Only report speech segment ended if we were speaking and now we're not
                    if (wasSpeaking && !isCurrentlySpeaking && DateTime.Now - _lastAudioTime > TimeSpan.FromMilliseconds(1000))
                    {
                        wasSpeaking = false;
                        // Don't spam the log with "Speech segment ended" messages
                        // SpeechDetected?.Invoke(this, "Speech segment ended");
                    }
                    else if (isCurrentlySpeaking && !wasSpeaking)
                    {
                        wasSpeaking = true;
                    }
                    
                    Thread.Sleep(200); // Check every 200ms instead of 100ms
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Audio monitoring error: {ex.Message}");
                }
            }
        }

        private float CalculateAudioLevel(byte[] buffer, int bytesRecorded)
        {
            if (bytesRecorded == 0) return 0f;
            
            float sum = 0f;
            int samples = bytesRecorded / 2; // 16-bit samples
            
            for (int i = 0; i < samples; i++)
            {
                if (i * 2 + 1 < bytesRecorded)
                {
                    short sample = (short)((buffer[i * 2 + 1] << 8) | buffer[i * 2]);
                    sum += Math.Abs(sample);
                }
            }
            
            return sum / samples / 32768f; // Normalize to 0-1
        }

        public bool IsCapturing => _isCapturing;
        public float CurrentAudioLevel => _lastAudioLevel;
    }
}
