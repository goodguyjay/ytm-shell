using System.IO;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace YoutubeMusicDesktop.Core;

public sealed class WebViewManager(WebView2 webView, ThemeManager themeManager)
{
    private ScriptInjector? _scriptInjector;

    private CssInjector? _cssInjector;

#if DEBUG
    private int _navigationCount;
#endif
    public bool CanGoBack => webView.CoreWebView2.CanGoBack;

    public bool CanGoForward => webView.CoreWebView2.CanGoForward;

    public event Action<bool>? PlayStateChanged;

    public event Action<TrackInfo>? TrackChanged;

    public event Action? PageNavigated;

    public async Task InitializeAsync()
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YoutubeMusicDesktop",
            "WebView2"
        );

        var options = new CoreWebView2EnvironmentOptions()
        {
            ScrollBarStyle = CoreWebView2ScrollbarStyle.FluentOverlay,
        };

        var env = await CoreWebView2Environment.CreateAsync(
            browserExecutableFolder: null,
            userDataFolder: userDataFolder,
            options: options
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

        _scriptInjector = new ScriptInjector(webView.CoreWebView2);
        _cssInjector = new CssInjector(webView.CoreWebView2, themeManager.Current.FileName);

        await _scriptInjector.RegisterAllAsync();
        await _cssInjector.RegisterAsync();

        webView.CoreWebView2.HistoryChanged += (_, _) => PageNavigated?.Invoke();

        webView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        webView.CoreWebView2.Navigate("https://music.youtube.com/");
    }

    public async Task ApplyThemeAsync(ThemeEntry theme)
    {
        if (_cssInjector is null)
            return;
        await _cssInjector.ClearAsync();
        await themeManager.ApplyAsync(theme);
        _cssInjector.SetTheme(theme.FileName);
        await _cssInjector.InjectAsync();
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

    public void GoBack() => webView.CoreWebView2.GoBack();

    public void GoForward() => webView.CoreWebView2.GoForward();

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
                await _cssInjector!.InjectAsync();

            PageNavigated?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to inject theme: {ex}");
        }
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string json;
        try
        {
            json = e.WebMessageAsJson;
        }
        catch
        {
            // discarded
            return;
        }

        try
        {
            // let the damn gc do its work
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return;

            if (!root.TryGetProperty("type", out var typeProp))
                return;

            switch (typeProp.GetString())
            {
                case "playState":
                    if (root.TryGetProperty("playing", out var playingProp))
                        PlayStateChanged?.Invoke(playingProp.GetBoolean());
                    break;

                case "trackChange":
                    if (root.TryGetProperty("title", out var titleProp))
                        TrackChanged?.Invoke(
                            new TrackInfo(
                                Title: titleProp.GetString() ?? string.Empty,
                                Artist: root.TryGetProperty("artist", out var a)
                                    ? a.GetString() ?? string.Empty
                                    : string.Empty,
                                ThumbnailUrl: root.TryGetProperty("thumbnailUrl", out var t)
                                    ? t.GetString() ?? string.Empty
                                    : string.Empty,
                                DurationSeconds: root.TryGetProperty("durationSeconds", out var d)
                                    ? d.GetInt32()
                                    : 0
                            )
                        );
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse web message: {ex.Message}");
        }
    }
}
