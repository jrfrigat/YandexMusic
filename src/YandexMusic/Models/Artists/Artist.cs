using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Artists;

/// <summary>Aggregate counts describing an artist's catalogue.</summary>
public sealed class ArtistCounts
{
    /// <summary>The number of tracks.</summary>
    public int Tracks { get; init; }

    /// <summary>The number of albums the artist released directly.</summary>
    public int DirectAlbums { get; init; }

    /// <summary>The number of albums the artist also appears on.</summary>
    public int AlsoAlbums { get; init; }

    /// <summary>The number of tracks the artist also appears on.</summary>
    public int AlsoTracks { get; init; }
}

/// <summary>An artist's popularity ratings over recent periods.</summary>
public sealed class ArtistRatings
{
    /// <summary>The rating over the last week.</summary>
    public int Week { get; init; }

    /// <summary>The rating over the last month.</summary>
    public int Month { get; init; }

    /// <summary>The rating over the last day.</summary>
    public int Day { get; init; }
}

/// <summary>An external link associated with an artist (for example a social network or website).</summary>
public sealed class ArtistLink
{
    /// <summary>The link title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The link target URL.</summary>
    public string Href { get; init; } = string.Empty;

    /// <summary>The kind of link (for example <c>social</c> or <c>official</c>).</summary>
    public string? Type { get; init; }
}

/// <summary>
/// An artist (performer or composer). The inline shape (inside tracks and albums) populates the
/// core fields; the artist endpoints populate the richer fields such as <see cref="Counts"/>,
/// <see cref="Ratings"/> and <see cref="Links"/>.
/// </summary>
public sealed class Artist
{
    /// <summary>The artist identifier.</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Id { get; init; } = string.Empty;

    /// <summary>The artist name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Whether this entry represents "various artists" rather than a single performer.</summary>
    public bool Various { get; init; }

    /// <summary>Whether the artist is credited as a composer.</summary>
    public bool Composer { get; init; }

    /// <summary>Whether the artist is available in the current region.</summary>
    public bool Available { get; init; }

    /// <summary>The artist cover image, when available.</summary>
    public Cover? Cover { get; init; }

    /// <summary>The Open Graph image URI template, when available.</summary>
    public string? OgImage { get; init; }

    /// <summary>The genres associated with the artist.</summary>
    public IReadOnlyList<string> Genres { get; init; } = [];

    /// <summary>Aggregate counts describing the artist's catalogue, when available.</summary>
    public ArtistCounts? Counts { get; init; }

    /// <summary>The artist's popularity ratings, when available.</summary>
    public ArtistRatings? Ratings { get; init; }

    /// <summary>External links associated with the artist, when available.</summary>
    public IReadOnlyList<ArtistLink>? Links { get; init; }

    /// <summary>The number of users who liked the artist.</summary>
    public int LikesCount { get; init; }

    /// <summary>Whether concert tickets are available for the artist.</summary>
    public bool TicketsAvailable { get; init; }

    /// <summary>The accent colours derived from the artist's imagery, when available.</summary>
    public DerivedColors? DerivedColors { get; init; }

    /// <summary>The artist biography, when available.</summary>
    public Description? Description { get; init; }

    /// <summary>The countries associated with the artist, when available.</summary>
    public IReadOnlyList<string>? Countries { get; init; }
}
