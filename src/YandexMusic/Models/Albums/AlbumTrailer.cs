using YandexMusic.Models.Artists;

namespace YandexMusic.Models.Albums;

/// <summary>The trailer payload for an album: the album, its artists and the trailer itself.</summary>
public sealed class AlbumTrailer
{
    /// <summary>The album the trailer belongs to, when present.</summary>
    public Album? Album { get; init; }

    /// <summary>The album artists, when present.</summary>
    public IReadOnlyList<Artist>? Artists { get; init; }

    /// <summary>The trailer information, including its tracks, when present.</summary>
    public TrailerInfo? Trailer { get; init; }
}
