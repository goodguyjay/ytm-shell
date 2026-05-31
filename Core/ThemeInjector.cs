using System.IO;
using Microsoft.Web.WebView2.Core;

namespace YoutubeMusicDesktop.Core;

public sealed class ThemeInjector(CoreWebView2 core, string cssFileName = "liquid-glass.css")
{
    private string _cssPath = Path.Combine(AppContext.BaseDirectory, "Themes", cssFileName);

    public void SetTheme(string cssFileName) =>
        _cssPath = Path.Combine(AppContext.BaseDirectory, "Themes", cssFileName);

    public async Task RegisterAsync()
    {
        var script = await BuildScriptAsync();
        await core.AddScriptToExecuteOnDocumentCreatedAsync(script);
    }

    public async Task InjectDeepAsync()
    {
        var script = await BuildScriptAsync();
        await core.ExecuteScriptAsync(script);
    }

    private async Task<string> BuildScriptAsync()
    {
        Console.WriteLine($"css path: {_cssPath}");

        var basePath = Path.Combine(AppContext.BaseDirectory, "Themes", "base");
        var componentsPath = Path.Combine(AppContext.BaseDirectory, "Themes", "components");

        var layers = new[]
        {
            Path.Combine(basePath, "variables.css"),
            Path.Combine(basePath, "reset.css"),
            Path.Combine(basePath, "scrollbar.css"),
            Path.Combine(componentsPath, "nav-bar.css"),
            Path.Combine(componentsPath, "player-bar.css"),
            Path.Combine(componentsPath, "progress-bar.css"),
            Path.Combine(componentsPath, "chips.css"),
            Path.Combine(componentsPath, "search.css"),
            Path.Combine(componentsPath, "cards.css"),
            Path.Combine(componentsPath, "main-view.css"),
            _cssPath,
        };

        var css = string.Concat(
            await Task.WhenAll(
                layers.Select(p =>
                    File.Exists(p) ? File.ReadAllTextAsync(p) : Task.FromResult(string.Empty)
                )
            )
        );

        var escaped = css.Replace("\\", "\\\\").Replace("`", "\\`").Replace("$", "\\$");

        var injectScript = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Scripts", "inject-theme.js")
        );

        return injectScript.Replace("{CSS}", escaped);
    }
}
