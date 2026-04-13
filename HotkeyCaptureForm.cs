using System.Windows.Forms;

namespace HotkeyPlayer;

public class HotkeyCaptureForm : Form
{
    private readonly Label _label;
    private readonly Button _btnConfirm;
    private readonly Button _btnCancel;
    private uint _modifiers;
    private uint _vk;

    public uint Modifiers => _modifiers;
    public uint Vk => _vk;
    public string HotkeyDisplay => HotkeyManager.FormatHotkey(_modifiers, _vk);

    public HotkeyCaptureForm()
    {
        Text = "设置快捷键";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        Width = 320;
        Height = 120;
        KeyPreview = true;

        _label = new Label
        {
            Text = "请按下快捷键组合...",
            Location = new Point(15, 15),
            Width = 280,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 10)
        };

        _btnConfirm = new Button
        {
            Text = "确定",
            Location = new Point(80, 50),
            Width = 75,
            DialogResult = DialogResult.OK
        };
        _btnConfirm.Click += (s, e) =>
        {
            if (_vk == 0)
            {
                MessageBox.Show("请先按下快捷键", "提示");
                DialogResult = DialogResult.None;
            }
        };

        _btnCancel = new Button
        {
            Text = "取消",
            Location = new Point(165, 50),
            Width = 75,
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange(new Control[] { _label, _btnConfirm, _btnCancel });
        AcceptButton = _btnConfirm;
        CancelButton = _btnCancel;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        _modifiers = 0;
        if (e.Modifiers.HasFlag(Keys.Control)) _modifiers |= HotkeyManager.MOD_CONTROL;
        if (e.Modifiers.HasFlag(Keys.Alt)) _modifiers |= HotkeyManager.MOD_ALT;
        if (e.Modifiers.HasFlag(Keys.Shift)) _modifiers |= HotkeyManager.MOD_SHIFT;

        // 获取实际的虚拟键码
        _vk = (uint)e.KeyValue;

        // 如果只按下了修饰键（Ctrl/Alt/Shift），不显示
        if (_vk != (uint)Keys.ControlKey &&
            _vk != (uint)Keys.Menu &&  // Alt
            _vk != (uint)Keys.ShiftKey)
        {
            _label.Text = HotkeyDisplay;
        }

        e.SuppressKeyPress = true;
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return true;
    }
}
