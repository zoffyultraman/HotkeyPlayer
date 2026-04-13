# HotkeyPlayer

全局快捷键音频播放器。支持为每个音频文件绑定一个全局快捷键，无论在任何全屏游戏还是应用程序中，按下快捷键即可立即播放对应音频。

## 功能

- **导入音频**：支持 WAV 和 MP3 格式
- **全局快捷键**：使用 Win32 `RegisterHotKey` API，真正的全局拦截，包括全屏 DirectX/OpenGL 游戏
- **右键设置快捷键**：支持 Ctrl/Alt/Shift/Win + 任意键组合
- **持久化配置**：快捷键和音频列表自动保存到 `%APPDATA%\HotkeyPlayer\settings.json`

## 使用方法

1. 下载最新 Release 中的 `HotkeyPlayer.exe`
2. 运行程序
3. 点击「导入音频」添加 WAV/MP3 文件
4. 右键点击音频条目 → 「设置快捷键」
5. 按下想要的快捷键组合（如 `Ctrl + F1`），点击确定
6. 最小化窗口，在任何应用中按下快捷键即可播放

## 技术栈

- **.NET 8** + **WinForms**
- **NAudio** - 音频播放
- **Win32 RegisterHotKey** - 全局快捷键
- **GitHub Actions** - 自动构建

## 构建

```bash
dotnet restore HotkeyPlayer.csproj
dotnet build HotkeyPlayer.csproj -c Release
```

构建产物位于 `HotkeyPlayer/bin/Release/net8.0-windows/publish/`
