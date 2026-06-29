using YandexMusic.Models.Albums;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Artists;

/// <summary>
/// A consolidated view of an artist: the artist itself together with their albums, popular tracks
/// and similar artists. Returned by the artist "brief info" endpoint.
/// </summary>
public sealed class ArtistBriefInfo
{
    /// <summary>The artist.</summary>
    public Artist Artist { get; init; } = new();

    /// <summary>The albums the artist released directly.</summary>
    public IReadOnlyList<Album> Albums { get; init; } = [];

    /// <summary>The albums the artist also appears on.</summary>
    public IReadOnlyList<Album> AlsoAlbums { get; init; } = [];

    /// <summary>The artist's most popular tracks.</summary>
    public IReadOnlyList<Track> PopularTracks { get; init; } = [];

    /// <summary>Artists similar to this one.</summary>
    public IReadOnlyList<Artist> SimilarArtists { get; init; } = [];
}
