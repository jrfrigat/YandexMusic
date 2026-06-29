using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;

namespace YandexMusic.Models.Metatags;

/// <summary>A metatag landing page: a curated podborka of artists, albums and playlists for a mood, activity, genre or epoch.</summary>
public sealed class Metatag
{
    /// <summary>The metatag identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The cover image URI template.</summary>
    public string? CoverUri { get; init; }

    /// <summary>The accent color associated with the landing page.</summary>
    public string? Color { get; init; }

    /// <summary>The landing page title.</summary>
    public MetatagTitle? Title { get; init; }

    /// <summary>Whether the current account has liked this metatag.</summary>
    public bool? Liked { get; init; }

    /// <summary>The identifier of the radio station that plays this metatag.</summary>
    public string? StationId { get; init; }

    /// <summary>The URL of the custom wave animation shown for this metatag.</summary>
    public string? CustomWaveAnimationUrl { get; init; }

    /// <summary>The featured artists.</summary>
    public IReadOnlyList<Artist> Artists { get; init; } = [];

    /// <summary>The featured albums.</summary>
    public IReadOnlyList<Album> Albums { get; init; } = [];

    /// <summary>The featured playlists.</summary>
    public IReadOnlyList<Playlist> Playlists { get; init; } = [];

    /// <summary>The sort options offered for the tracks listing.</summary>
    public IReadOnlyList<MetatagSortByValue> TracksSortByValues { get; init; } = [];

    /// <summary>The sort options offered for the albums listing.</summary>
    public IReadOnlyList<MetatagSortByValue> AlbumsSortByValues { get; init; } = [];

    /// <summary>The sort options offered for the playlists listing.</summary>
    public IReadOnlyList<MetatagSortByValue> PlaylistsSortByValues { get; init; } = [];
}
