using System.IO;

namespace YoutubeMusicDesktop.Core;

public record ThemeEntry(string DisplayName, string FileName);

public sealed class ThemeManager
{
    private static readonly ThemeEntry[] Themes =
    [
        new("Liquid Glass", "liquid-glass.css"),
        new("Mica / Fluent", "mica-fluent.css"),
        new("Aurora Dark", "aurora-dark.css"),
        new("Depth", "depth.css"),
        new("Cyberpunk", "cyberpunk.css"),
    ];

    private static readonly string PrefsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "YoutubeMusicDesktop",
        "theme.txt"
    );

    public IReadOnlyList<ThemeEntry> Available => Themes;

    public ThemeEntry Current { get; private set; } = Themes[0];

    private static ThemeEntry LoadSaved()
    {
        try
        {
            if (File.Exists(PrefsPath))
            {
                var saved = File.ReadAllText(PrefsPath).Trim();
                var match = Themes.FirstOrDefault(t => t.FileName == saved);
                if (match is not null)
                    return match;
            }
        }
        catch
        {
            // ignored
        }

        return Themes[0];
    }

    public Task ApplyAsync(ThemeEntry theme)
    {
        Current = theme;
        SavePreference(theme);
        return Task.CompletedTask;
    }

    private static void SavePreference(ThemeEntry theme)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefsPath)!);
            File.WriteAllText(PrefsPath, theme.FileName);
        }
        catch
        {
            // ignored
        }
    }
}
