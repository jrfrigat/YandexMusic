using YandexMusic.Models.Albums;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Artists;

/// <summary>A page of an artist's tracks.</summary>
public sealed class ArtistTracksPage
{
    /// <summary>Pagination information.</summary>
    public Pager Pager { get; init; } = new();

    /// <summary>The tracks on this page.</summary>
    public IReadOnlyList<Track> Tracks { get; init; } = [];
}

/// <summary>A page of an artist's directly-released albums.</summary>
public sealed class ArtistAlbumsPage
{
    /// <summary>Pagination information.</summary>
    public Pager Pager { get; init; } = new();

    /// <summary>The albums on this page.</summary>
    public IReadOnlyList<Album> Albums { get; init; } = [];
}
