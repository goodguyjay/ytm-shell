namespace YoutubeMusicDesktop.Core;

public record TrackInfo(
    string Title,
    string Artist,
    string ThumbnailUrl = "",
    int DurationSeconds = 0
);
