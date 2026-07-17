# ytm-shell

## i'll fix this readme later ;) scope has changed drastically

A native Windows desktop shell for YouTube Music, built with WPF and WebView2. Replaces the browser tab with a proper desktop experience: liquid glass UI, taskbar controls, keyboard shortcuts, and a theming system designed to get out of the way.

---

## Why

Most desktop players rely on heavy, isolated browser runtimes that act as resource hogs and lack true OS integration. ytm-shell takes a different approach by leveraging the native WebView2 runtime already baked into Windows, wrapping it in a lightweight WPF shell.

This architecture unlocks:
- Deep Windows Integration: Native performance, instant startup, and minimal memory footprint without the Electron overhead.
- Advanced Theming System: Built from the ground up to inject custom CSS and JavaScript, allowing granular control over the UI/UX that standard web wrappers can't achieve.
- Extensibility: Built for developers who want to tinker, tweak styles, and build custom features directly on top of the YouTube Music interface.

---

## Requirements

- Windows 10 or later
- [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (included with most Windows installations)

---

## Getting Started

### Running from source

```bash
git clone https://github.com/yourusername/ytm-shell
cd ytm-shell
dotnet run
```

### Building

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

The output will be in `bin/Release/net10.0-windows/win-x64/publish/`

---

## Project Structure

```
YoutubeMusicDesktop/
├── Components/         # WPF UI components (title bar, taskbar icons)
├── Core/               # WebView2 management, theme injection, player controls
│   ├── WebViewManager.cs
│   ├── ThemeInjector.cs
│   └── ThemeManager.cs
├── Scripts/            # JavaScript injected into the YouTube Music web interface
│   ├── player-controls.js
│   ├── player-state.js
│   ├── inject-theme.js
│   └── attach-shadow-patch.js
└── Themes/
    ├── base/           # CSS reset, variables, scrollbar
    ├── components/     # Player bar, nav bar, cards, chips, search
    └── liquid-glass.css  # Default theme
```

---

## Themes

ytm-shell ships with multiple themes. The active theme is injected into the YouTube Music interface on load. In debug builds, a theme selector is available in the title bar.

| Theme        | Status |
|--------------|--------|
| Liquid Glass | wip    |
| Aurora Dark  | later  |
| Mica Fluent  | later  |
| Depth        | later  |
| Cyberpunk    | later  |

---

## License

[MIT](LICENSE.md)