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

    /// <summary>The call-to-action button of the artist page, when present.</summary>
    public ActionButton? ActionButton { get; init; }

    /// <summary>The background image URL of the artist page.</summary>
    public string? BackgroundImageUrl { get; init; }

    /// <summary>The identifier of the background video of the artist page.</summary>
    public string? BackgroundVideoId { get; init; }

    /// <summary>The URL of the background video of the artist page.</summary>
    public string? BackgroundVideoUrl { get; init; }

    /// <summary>The Bandlink page listing playlists that feature the artist.</summary>
    public ArtistLink? BandlinkScannerLink { get; init; }

    /// <summary>The artist's clips. Only sent when the request asks for the clip data format.</summary>
    public IReadOnlyList<Clip> Clips { get; init; } = [];

    /// <summary>"My wave" seeded from the artist, when the page promotes one.</summary>
    public CustomWave? CustomWave { get; init; }

    /// <summary>Whether the artist has a trailer.</summary>
    public bool? HasTrailer { get; init; }

    /// <summary>Links to the artist's pages elsewhere.</summary>
    public IReadOnlyList<ArtistLink> Links { get; init; } = [];

    /// <summary>The artist's announced, not yet released album.</summary>
    public UpcomingAlbum? UpcomingAlbum { get; init; }
}
