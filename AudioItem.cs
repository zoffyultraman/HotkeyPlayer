using System.Text.Json.Serialization;

namespace HotkeyPlayer;

public class AudioItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    [JsonIgnore]
    public string? HotkeyDisplay => string.IsNullOrEmpty(Hotkey) ? "(未设置)" : Hotkey;

    public string? Hotkey { get; set; }

    [JsonIgnore]
    public int HotkeyId { get; set; }
}
