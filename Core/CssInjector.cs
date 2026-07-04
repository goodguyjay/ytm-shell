using System.IO;
using Microsoft.Web.WebView2.Core;

namespace YoutubeMusicDesktop.Core;

public sealed class CssInjector(CoreWebView2 core, string initialTheme = "liquid-glass.css")
{
    private string _cssFileName = initialTheme;

    private string CssPath => Path.Combine(AppContext.BaseDirectory, "Themes", _cssFileName);

    public void SetTheme(string cssFileName) => _cssFileName = cssFileName;

    public async Task RegisterAsync()
    {
        if (string.IsNullOrEmpty(_cssFileName))
            return;

        await core.AddScriptToExecuteOnDocumentCreatedAsync(await BuildScriptAsync());
    }

    public async Task InjectAsync() 
    {
        if (string.IsNullOrEmpty(_cssFileName))
            return;
        
        await core.ExecuteScriptAsync(await BuildScriptAsync());
    }
    
    public Task ClearAsync() =>
        core.ExecuteScriptAsync("document.querySelector('style[data-ytmshell]')?.remove();");

    private async Task<string> BuildScriptAsync()
    {
        Console.WriteLine($"css path: {CssPath}");

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
            CssPath,
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
            Path.Combine(AppContext.BaseDirectory, "Themes", "inject-theme.js")
        );

        return injectScript.Replace("{CSS}", escaped);
    }
}
