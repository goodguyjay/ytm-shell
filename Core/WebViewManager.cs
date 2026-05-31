using System.IO;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace YoutubeMusicDesktop.Core;

public sealed class WebViewManager(WebView2 webView, ThemeManager themeManager)
{
    private ThemeInjector? _themeInjector;

#if DEBUG
    private int _navigationCount;
#endif

    public async Task InitializeAsync()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YoutubeMusicDesktop",
            "WebView2"
        );

        var env = await CoreWebView2Environment.CreateAsync(
            browserExecutableFolder: null,
            userDataFolder: userDataFolder
        );

        await webView.EnsureCoreWebView2Async(env);

        var settings = webView.CoreWebView2.Settings;
#if DEBUG
        settings.AreDevToolsEnabled = true;
#else
        settings.AreDevToolsEnabled = false;
#endif
        settings.AreDefaultContextMenusEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.IsSwipeNavigationEnabled = false;

        _themeInjector = new ThemeInjector(webView.CoreWebView2, themeManager.Current.FileName);

        await _themeInjector.RegisterAsync();
        await RegisterPlayerControlsAsync();

        webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        webView.CoreWebView2.Navigate("https://music.youtube.com/");
    }

    public async Task ApplyThemeAsync(ThemeEntry theme)
    {
        if (_themeInjector is null)
            return;
        await themeManager.ApplyAsync(theme, _themeInjector);
    }

    public async Task<bool> IsPlayingAsync()
    {
        var result = await webView.CoreWebView2.ExecuteScriptAsync(
            "window.__ytmControls?.isPlaying() ?? false"
        );
        return result == "true";
    }

    public Task TogglePlayPauseAsync() =>
        webView.CoreWebView2.ExecuteScriptAsync("window.__ytmControls?.togglePlayPause()");

    public Task PreviousAsync() =>
        webView.CoreWebView2.ExecuteScriptAsync("window.__ytmControls?.previous()");

    public Task NextAsync() =>
        webView.CoreWebView2.ExecuteScriptAsync("window.__ytmControls?.next()");

    private async Task RegisterPlayerControlsAsync()
    {
        var script = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Scripts", "player-controls.js")
        );
        await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
    }

    private async void OnNavigationCompleted(
        object? sender,
        CoreWebView2NavigationCompletedEventArgs e
    )
    {
        try
        {
#if DEBUG
            _navigationCount++;
            Console.WriteLine($"Navigation #{_navigationCount} - {DateTime.Now:HH:mm:ss}");
#endif
            if (e.IsSuccess)
                await _themeInjector!.InjectDeepAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to inject theme: {ex}");
        }
    }
}
