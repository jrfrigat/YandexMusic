namespace YandexMusic.Models.Concerts;

/// <summary>The set of concerts associated with an artist.</summary>
public sealed class ArtistConcerts
{
    /// <summary>The artist display title, when present.</summary>
    public string? ArtistTitle { get; init; }

    /// <summary>The artist's concerts.</summary>
    public IReadOnlyList<Concert> Concerts { get; init; } = [];
}
