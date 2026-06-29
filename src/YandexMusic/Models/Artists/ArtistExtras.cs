namespace YandexMusic.Models.Artists;

/// <summary>The result of the artist "similar" endpoint: an artist together with similar performers.</summary>
public sealed class ArtistSimilar
{
    /// <summary>The artist the request was made for, when echoed back.</summary>
    public Artist? Artist { get; init; }

    /// <summary>Artists similar to the requested one.</summary>
    public IReadOnlyList<Artist>? SimilarArtists { get; init; }
}

/// <summary>The result of the artist "artist-links" endpoint: the external links of an artist.</summary>
public sealed class ArtistLinks
{
    /// <summary>The external links associated with the artist.</summary>
    public IReadOnlyList<ArtistLink>? Links { get; init; }
}

/// <summary>Aggregate listener statistics for an artist.</summary>
public sealed class Stats
{
    /// <summary>The number of listeners over the last month.</summary>
    public int LastMonthListeners { get; init; }

    /// <summary>The change in last-month listeners compared with the previous period.</summary>
    public int LastMonthListenersDelta { get; init; }
}

/// <summary>The result of the artist "about-artist" endpoint: a descriptive profile of an artist.</summary>
public sealed class ArtistAbout
{
    /// <summary>The artist, when present.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The artist's listener statistics, when present.</summary>
    public Stats? Stats { get; init; }

    /// <summary>The artist description, when present.</summary>
    public string? Description { get; init; }

    /// <summary>The artist's external links, when present.</summary>
    public IReadOnlyList<ArtistLink>? Links { get; init; }

    /// <summary>The artist cover images, when present.</summary>
    public IReadOnlyList<Cover>? Covers { get; init; }

    /// <summary>The kind of artist (for example a performer or a podcaster), when present.</summary>
    public string? ArtistType { get; init; }
}

/// <summary>The availability flag of an artist's trailer, as reported by the artist "info" endpoint.</summary>
public sealed class ArtistTrailerStatus
{
    /// <summary>Whether a trailer is available for the artist.</summary>
    public bool? Available { get; init; }
}

/// <summary>The result of the artist "info" endpoint: a rich profile of an artist.</summary>
public sealed class ArtistInfo
{
    /// <summary>The artist, when present.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The number of users who liked the artist, when present.</summary>
    public int? LikesCount { get; init; }

    /// <summary>The artist's listener statistics, when present.</summary>
    public Stats? Stats { get; init; }

    /// <summary>The availability status of the artist's trailer, when present.</summary>
    public ArtistTrailerStatus? Trailer { get; init; }

    /// <summary>The artist cover images, when present.</summary>
    public IReadOnlyList<Cover>? Covers { get; init; }

    /// <summary>The artist description, when present.</summary>
    public string? Description { get; init; }

    /// <summary>The kind of artist, when present.</summary>
    public string? ArtistType { get; init; }
}

/// <summary>The result of the artist "trailer" endpoint: the artist together with its trailer.</summary>
public sealed class ArtistTrailer
{
    /// <summary>The artist, when present.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The trailer attached to the artist, when present.</summary>
    public TrailerInfo? Trailer { get; init; }
}
