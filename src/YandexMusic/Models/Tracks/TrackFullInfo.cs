using YandexMusic.Models.Artists;

namespace YandexMusic.Models.Tracks;

/// <summary>Extended information about a track, including related tracks and aliases.</summary>
public sealed class TrackFullInfo
{
    /// <summary>The track itself, when present.</summary>
    public Track? Track { get; init; }

    /// <summary>Tracks similar to this one.</summary>
    public IReadOnlyList<Track>? SimilarTracks { get; init; }

    /// <summary>Other albums this track also appears on.</summary>
    public IReadOnlyList<Track>? AlsoInAlbums { get; init; }

    /// <summary>Alternative titles for the track.</summary>
    public IReadOnlyList<string>? Aliases { get; init; }

    /// <summary>The artists credited on the track.</summary>
    public IReadOnlyList<Artist>? Artists { get; init; }
}
