using DiscordRPC;
using DiscordRPC.Logging;

namespace YoutubeMusicDesktop.Core;

public sealed class DiscordService : IDisposable
{
    private const string ClientId = "1520813998444908696";
    private const int DebounceMs = 1_000;
    private const int PauseDelayMs = 30_000;

    private readonly DiscordRpcClient _client;
    private TrackInfo? _track;
    private bool _playing;
    private Timer? _debounce;
    private Timer? _pauseTimer;

    public DiscordService()
    {
        _client = new DiscordRpcClient(ClientId)
        {
            Logger = new ConsoleLogger { Level = LogLevel.Warning },
        };
        _client.Initialize();
    }

    public void OnTrackChanged(TrackInfo track)
    {
        _track = track;
        ScheduleUpdate();
    }

    public void OnPlayStateChanged(bool playing)
    {
        _playing = playing;

        _pauseTimer?.Dispose();
        _pauseTimer = null;

        if (!playing)
        {
            _pauseTimer = new Timer(
                _ =>
                {
                    _client.ClearPresence();
                },
                null,
                PauseDelayMs,
                Timeout.Infinite
            );
        }

        ScheduleUpdate();
    }

    private void ScheduleUpdate()
    {
        _debounce?.Dispose();
        _debounce = new Timer(_ => UpdatePresence(), null, DebounceMs, Timeout.Infinite);
    }

    private void UpdatePresence()
    {
        if (_track is null)
            return;

        _client.SetPresence(
            new RichPresence
            {
                Details = Clamp(_track.Title),
                State = Clamp(_track.Artist),
                Timestamps = _playing ? Timestamps.Now : null,
                Assets = new Assets
                {
                    LargeImageKey = string.IsNullOrEmpty(_track.ThumbnailUrl)
                        ? "logo"
                        : _track.ThumbnailUrl,
                    LargeImageText = "Youtube Music",
                    SmallImageKey = _playing ? "playing" : "paused",
                    SmallImageText = _playing ? "Playing" : "Paused",
                },
            }
        );
    }

    private static string Clamp(string s)
    {
        if (string.IsNullOrEmpty(s))
            s = " "; // avoids crashing

        if (s.Length < 2)
            s = s.PadRight(2, '\u200b'); // zero-width space

        if (s.Length > 128)
            s = s[..125] + "...";

        return s;
    }

    public void Dispose()
    {
        _debounce?.Dispose();
        _pauseTimer?.Dispose();
        _client.Dispose();
    }
}
