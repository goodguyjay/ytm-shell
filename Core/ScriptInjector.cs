using System.IO;
using Microsoft.Web.WebView2.Core;

namespace YoutubeMusicDesktop.Core;

public sealed class ScriptInjector(CoreWebView2 core)
{
    public async Task RegisterAllAsync()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Scripts");
        var paths = Directory.GetFiles(dir, "*.js").OrderBy(f => f).ToList();

        var scripts = await Task.WhenAll(paths.Select(async p => await File.ReadAllTextAsync(p)));

        var combined = string.Join("\n", scripts.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(combined))
            await core.AddScriptToExecuteOnDocumentCreatedAsync(combined);
    }
}
