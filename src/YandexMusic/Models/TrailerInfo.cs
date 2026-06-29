using YandexMusic.Models.Tracks;

namespace YandexMusic.Models;

/// <summary>A trailer (a short preview) attached to an album, artist or playlist.</summary>
public sealed class TrailerInfo
{
    /// <summary>The trailer title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The tracks that make up the trailer.</summary>
    public IReadOnlyList<Track>? Tracks { get; init; }
}
