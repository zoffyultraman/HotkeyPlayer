using NAudio.Wave;

namespace HotkeyPlayer;

public class AudioManager : IDisposable
{
    private WaveOutEvent? _waveOut;
    private AudioFileReader? _audioFile;
    private bool _disposed;

    public void Play(string filePath)
    {
        try
        {
            Stop();

            _audioFile = new AudioFileReader(filePath);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_audioFile);
            _waveOut.Play();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"播放失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void Stop()
    {
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _waveOut = null;

        _audioFile?.Dispose();
        _audioFile = null;
    }

    public static bool IsValidAudioFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".wav" || ext == ".mp3";
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}
