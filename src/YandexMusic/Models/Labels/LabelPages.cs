using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;

namespace YandexMusic.Models.Labels;

/// <summary>A page of albums belonging to a label.</summary>
public sealed class LabelAlbums
{
    /// <summary>The albums on the current page.</summary>
    public IReadOnlyList<Album> Albums { get; init; } = [];

    /// <summary>The pagination information, when present.</summary>
    public Pager? Pager { get; init; }
}

/// <summary>A page of artists belonging to a label.</summary>
public sealed class LabelArtists
{
    /// <summary>The artists on the current page.</summary>
    public IReadOnlyList<Artist> Artists { get; init; } = [];

    /// <summary>The pagination information, when present.</summary>
    public Pager? Pager { get; init; }
}
