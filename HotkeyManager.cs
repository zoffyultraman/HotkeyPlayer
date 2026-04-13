using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HotkeyPlayer;

public class HotkeyManager : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;

    // Modifiers
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    private readonly Dictionary<int, AudioItem> _hotkeyMap = new();
    private IntPtr _hWnd;
    private int _nextId = 1;
    private bool _disposed;

    public event Action<AudioItem>? HotkeyTriggered;

    public void Initialize(IntPtr hWnd)
    {
        _hWnd = hWnd;
    }

    public int RegisterHotkey(AudioItem item, uint modifiers, uint vk)
    {
        if (_hWnd == IntPtr.Zero) return -1;

        // Unregister old hotkey if exists
        if (item.HotkeyId != 0)
        {
            UnregisterHotKey(_hWnd, item.HotkeyId);
            _hotkeyMap.Remove(item.HotkeyId);
        }

        int id = _nextId++;
        if (RegisterHotKey(_hWnd, id, modifiers | MOD_NOREPEAT, vk))
        {
            item.HotkeyId = id;
            _hotkeyMap[id] = item;
            return id;
        }
        return -1;
    }

    public void UnregisterHotkey(int hotkeyId)
    {
        if (_hWnd != IntPtr.Zero && hotkeyId != 0)
        {
            UnregisterHotKey(_hWnd, hotkeyId);
            _hotkeyMap.Remove(hotkeyId);
        }
    }

    public void UnregisterAll()
    {
        foreach (var id in _hotkeyMap.Keys.ToList())
        {
            UnregisterHotkey(id);
        }
    }

    public void ProcessMessage(int hotkeyId)
    {
        if (_hotkeyMap.TryGetValue(hotkeyId, out var item))
        {
            HotkeyTriggered?.Invoke(item);
        }
    }

    public static (uint modifiers, uint vk) ParseHotkeyString(string? hotkey)
    {
        if (string.IsNullOrEmpty(hotkey)) return (0, 0);

        uint modifiers = 0;
        uint vk = 0;

        var parts = hotkey.Split('+');
        foreach (var part in parts)
        {
            var trimmed = part.Trim().ToUpperInvariant();
            switch (trimmed)
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "ALT":
                    modifiers |= MOD_ALT;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    if (trimmed.Length == 1)
                    {
                        vk = (uint)trimmed[0];
                    }
                    else if (Enum.TryParse<Keys>(trimmed, true, out var key))
                    {
                        vk = (uint)key;
                    }
                    break;
            }
        }

        return (modifiers, vk);
    }

    public static string FormatHotkey(uint modifiers, uint vk)
    {
        var parts = new List<string>();
        if ((modifiers & MOD_CONTROL) != 0) parts.Add("Ctrl");
        if ((modifiers & MOD_ALT) != 0) parts.Add("Alt");
        if ((modifiers & MOD_SHIFT) != 0) parts.Add("Shift");
        if ((modifiers & MOD_WIN) != 0) parts.Add("Win");

        var keyName = ((Keys)vk).ToString();
        parts.Add(keyName);

        return string.Join(" + ", parts);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            UnregisterAll();
            _disposed = true;
        }
    }
}
