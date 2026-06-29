using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Metatags;

/// <summary>A paged albums listing for a metatag landing page.</summary>
public sealed class MetatagAlbums
{
    /// <summary>The metatag identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The cover image URI template.</summary>
    public string? CoverUri { get; init; }

    /// <summary>The accent color associated with the landing page.</summary>
    public string? Color { get; init; }

    /// <summary>The landing page title.</summary>
    public MetatagTitle? Title { get; init; }

    /// <summary>The identifier of the radio station that plays this metatag.</summary>
    public string? StationId { get; init; }

    /// <summary>The pagination information for the listing.</summary>
    public Pager? Pager { get; init; }

    /// <summary>The albums on the current page.</summary>
    public IReadOnlyList<Album> Albums { get; init; } = [];

    /// <summary>The sort options offered for the listing.</summary>
    public IReadOnlyList<MetatagSortByValue> SortByValues { get; init; } = [];
}

/// <summary>A paged artists listing for a metatag landing page.</summary>
public sealed class MetatagArtists
{
    /// <summary>The metatag identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The cover image URI template.</summary>
    public string? CoverUri { get; init; }

    /// <summary>The accent color associated with the landing page.</summary>
    public string? Color { get; init; }

    /// <summary>The landing page title.</summary>
    public MetatagTitle? Title { get; init; }

    /// <summary>The identifier of the radio station that plays this metatag.</summary>
    public string? StationId { get; init; }

    /// <summary>The pagination information for the listing.</summary>
    public Pager? Pager { get; init; }

    /// <summary>The artists on the current page, each with a few popular tracks.</summary>
    public IReadOnlyList<MetatagArtistEntry> Artists { get; init; } = [];

    /// <summary>The sort options offered for the listing.</summary>
    public IReadOnlyList<MetatagSortByValue> SortByValues { get; init; } = [];
}

/// <summary>An artist entry in a metatag artists listing, paired with a few of the artist's popular tracks.</summary>
public sealed class MetatagArtistEntry
{
    /// <summary>The artist.</summary>
    public Artist? Artist { get; init; }

    /// <summary>A few of the artist's most popular tracks.</summary>
    public IReadOnlyList<Track> PopularTracks { get; init; } = [];
}

/// <summary>A paged playlists listing for a metatag landing page.</summary>
public sealed class MetatagPlaylists
{
    /// <summary>The metatag identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The cover image URI template.</summary>
    public string? CoverUri { get; init; }

    /// <summary>The accent color associated with the landing page.</summary>
    public string? Color { get; init; }

    /// <summary>The landing page title.</summary>
    public MetatagTitle? Title { get; init; }

    /// <summary>The identifier of the radio station that plays this metatag.</summary>
    public string? StationId { get; init; }

    /// <summary>The pagination information for the listing.</summary>
    public Pager? Pager { get; init; }

    /// <summary>The playlists on the current page.</summary>
    public IReadOnlyList<Playlist> Playlists { get; init; } = [];

    /// <summary>The sort options offered for the listing.</summary>
    public IReadOnlyList<MetatagSortByValue> SortByValues { get; init; } = [];
}
