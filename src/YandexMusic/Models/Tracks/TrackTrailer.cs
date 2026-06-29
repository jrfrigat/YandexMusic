namespace YandexMusic.Models.Tracks;

/// <summary>A trailer associated with a track (for example a podcast or audiobook preview).</summary>
public sealed class TrackTrailer
{
    /// <summary>The trailer title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The trailer track, when present.</summary>
    public Track? Track { get; init; }
}
