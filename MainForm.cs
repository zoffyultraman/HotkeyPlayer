namespace HotkeyPlayer;

public class MainForm : Form
{
    private readonly ListView _listView;
    private readonly ColumnHeader _colName;
    private readonly ColumnHeader _colHotkey;
    private readonly Button _btnAdd;
    private readonly Button _btnRemove;
    private readonly Button _btnTest;

    private readonly HotkeyManager _hotkeyManager;
    private readonly AudioManager _audioManager;
    private readonly SettingsManager _settingsManager;

    private const int WM_HOTKEY = 0x0312;

    public MainForm()
    {
        MessageBox.Show("MainForm 构造函数开始");
        Text = "HotkeyPlayer - 全局快捷键音频播放器";
        Width = 550;
        Height = 400;
        StartPosition = FormStartPosition.CenterScreen;

        _hotkeyManager = new HotkeyManager();
        _audioManager = new AudioManager();
        _settingsManager = new SettingsManager();

        // ListView
        _listView = new ListView
        {
            Dock = DockStyle.Top,
            Height = 280,
            FullRowSelect = true,
            View = View.Details,
            SmallImageList = new ImageList()
        };

        _colName = new ColumnHeader { Text = "音频名称", Width = 250 };
        _colHotkey = new ColumnHeader { Text = "快捷键", Width = 200 };
        _listView.Columns.AddRange(new[] { _colName, _colHotkey });

        _listView.ContextMenuStrip = CreateContextMenu();
        _listView.MouseDoubleClick += (s, e) => TestSelected();

        // Buttons
        _btnAdd = new Button
        {
            Text = "导入音频",
            Location = new Point(15, 295),
            Width = 100
        };
        _btnAdd.Click += AddAudioFile;

        _btnRemove = new Button
        {
            Text = "删除",
            Location = new Point(125, 295),
            Width = 80
        };
        _btnRemove.Click += RemoveSelected;

        _btnTest = new Button
        {
            Text = "测试播放",
            Location = new Point(215, 295),
            Width = 80
        };
        _btnTest.Click += TestSelected;

        var label = new Label
        {
            Text = "提示：双击条目可测试播放，右键可设置快捷键",
            Location = new Point(15, 330),
            ForeColor = Color.Gray
        };

        Controls.AddRange(new Control[] { _listView, _btnAdd, _btnRemove, _btnTest, label });

        _settingsManager.Load();
        _hotkeyManager.Initialize(Handle);
        _hotkeyManager.HotkeyTriggered += OnHotkeyTriggered;

        LoadAudioItems();
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();
        var setHotkeyItem = new ToolStripMenuItem("设置快捷键");
        setHotkeyItem.Click += (s, e) => SetHotkeyForSelected();
        var removeHotkeyItem = new ToolStripMenuItem("清除快捷键");
        removeHotkeyItem.Click += (s, e) => ClearHotkeyForSelected();
        var playItem = new ToolStripMenuItem("测试播放");
        playItem.Click += (s, e) => TestSelected();

        menu.Items.AddRange(new[] { setHotkeyItem, removeHotkeyItem, playItem });
        return menu;
    }

    private void LoadAudioItems()
    {
        _listView.Items.Clear();
        foreach (var item in _settingsManager.AudioItems)
        {
            var lvi = new ListViewItem(item.Name);
            lvi.SubItems.Add(item.HotkeyDisplay ?? "(未设置)");
            lvi.Tag = item;
            _listView.Items.Add(lvi);

            // Register hotkey if exists
            if (!string.IsNullOrEmpty(item.Hotkey))
            {
                var (modifiers, vk) = HotkeyManager.ParseHotkeyString(item.Hotkey);
                if (vk != 0)
                {
                    _hotkeyManager.RegisterHotkey(item, modifiers, vk);
                }
            }
        }
    }

    private void AddAudioFile(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "音频文件|*.wav;*.mp3|WAV文件|*.wav|MP3文件|*.mp3",
            Multiselect = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            foreach (var filePath in dialog.FileNames)
            {
                if (!AudioManager.IsValidAudioFile(filePath))
                    continue;

                var item = new AudioItem
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath
                };

                _settingsManager.AddAudioItem(item);

                var lvi = new ListViewItem(item.Name);
                lvi.SubItems.Add("(未设置)");
                lvi.Tag = item;
                _listView.Items.Add(lvi);
            }
        }
    }

    private void RemoveSelected(object? sender, EventArgs e)
    {
        if (_listView.SelectedItems.Count == 0) return;

        var item = _listView.SelectedItems[0].Tag as AudioItem;
        if (item == null) return;

        if (item.HotkeyId != 0)
        {
            _hotkeyManager.UnregisterHotkey(item.HotkeyId);
        }

        _settingsManager.RemoveAudioItem(item.Id);
        _listView.Items.Remove(_listView.SelectedItems[0]);
    }

    private void TestSelected(object? sender, EventArgs e)
    {
        TestSelected();
    }

    private void TestSelected()
    {
        if (_listView.SelectedItems.Count == 0) return;
        var item = _listView.SelectedItems[0].Tag as AudioItem;
        if (item == null) return;

        _audioManager.Play(item.FilePath);
    }

    private void SetHotkeyForSelected()
    {
        if (_listView.SelectedItems.Count == 0) return;
        var item = _listView.SelectedItems[0].Tag as AudioItem;
        if (item == null) return;

        using var form = new HotkeyCaptureForm();
        if (form.ShowDialog() == DialogResult.OK && form.Vk != 0)
        {
            var hotkeyString = form.HotkeyDisplay;
            var (modifiers, vk) = (form.Modifiers, form.Vk);

            var id = _hotkeyManager.RegisterHotkey(item, modifiers, vk);
            if (id < 0)
            {
                MessageBox.Show("快捷键注册失败，可能与其他程序冲突", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            item.Hotkey = hotkeyString;
            _settingsManager.UpdateAudioItem(item);

            _listView.SelectedItems[0].SubItems[1].Text = hotkeyString;
        }
    }

    private void ClearHotkeyForSelected()
    {
        if (_listView.SelectedItems.Count == 0) return;
        var item = _listView.SelectedItems[0].Tag as AudioItem;
        if (item == null) return;

        if (item.HotkeyId != 0)
        {
            _hotkeyManager.UnregisterHotkey(item.HotkeyId);
            item.HotkeyId = 0;
        }

        item.Hotkey = null;
        _settingsManager.UpdateAudioItem(item);
        _listView.SelectedItems[0].SubItems[1].Text = "(未设置)";
    }

    private void OnHotkeyTriggered(AudioItem item)
    {
        // 在 UI 线程播放
        BeginInvoke(() => _audioManager.Play(item.FilePath));
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY)
        {
            int hotkeyId = m.WParam.ToInt32();
            _hotkeyManager.ProcessMessage(hotkeyId);
        }
        base.WndProc(ref m);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _hotkeyManager.Dispose();
        _audioManager.Dispose();
        base.OnFormClosing(e);
    }
}
