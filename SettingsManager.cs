using System.Text.Json;

namespace HotkeyPlayer;

public class SettingsManager
{
    private readonly string _settingsPath;
    private AppSettings _settings = new();

    public SettingsManager()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HotkeyPlayer");

        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");
    }

    public List<AudioItem> AudioItems => _settings.AudioItems;

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch
        {
            _settings = new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch { }
    }

    public void AddAudioItem(AudioItem item)
    {
        _settings.AudioItems.Add(item);
        Save();
    }

    public void RemoveAudioItem(string id)
    {
        _settings.AudioItems.RemoveAll(x => x.Id == id);
        Save();
    }

    public void UpdateAudioItem(AudioItem item)
    {
        var index = _settings.AudioItems.FindIndex(x => x.Id == item.Id);
        if (index >= 0)
        {
            _settings.AudioItems[index] = item;
            Save();
        }
    }
}

public class AppSettings
{
    public List<AudioItem> AudioItems { get; set; } = new();
}
