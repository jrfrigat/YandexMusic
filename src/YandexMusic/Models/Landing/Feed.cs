using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Landing;

/// <summary>The personalized feed shown on the landing page, including smart playlists and a day-by-day event timeline.</summary>
public sealed class Feed
{
    /// <summary>Whether more historical event days can be requested.</summary>
    public bool CanGetMoreEvents { get; init; }

    /// <summary>Whether the seasonal "pumpkin" theme is active.</summary>
    public bool Pumpkin { get; init; }

    /// <summary>Whether the user has completed the onboarding wizard.</summary>
    public bool IsWizardPassed { get; init; }

    /// <summary>The auto-generated playlists surfaced in the feed.</summary>
    public IReadOnlyList<GeneratedPlaylist> GeneratedPlaylists { get; init; } = [];

    /// <summary>The promotional headlines for the feed.</summary>
    public IReadOnlyList<string> Headlines { get; init; } = [];

    /// <summary>The current day, formatted as <c>YYYY-MM-DD</c>.</summary>
    public string Today { get; init; } = string.Empty;

    /// <summary>The event timeline, grouped by day.</summary>
    public IReadOnlyList<Day> Days { get; init; } = [];

    /// <summary>The revision marker for the next page of days, formatted as <c>YYYY-MM-DD</c>, when present.</summary>
    public string? NextRevision { get; init; }
}

/// <summary>A single day in the feed timeline, with its events and tracks to play.</summary>
public sealed class Day
{
    /// <summary>The day this entry covers, formatted as <c>YYYY-MM-DD</c>.</summary>
    [JsonPropertyName("day")]
    public string DayDate { get; init; } = string.Empty;

    /// <summary>The events that occurred on this day.</summary>
    public IReadOnlyList<Event> Events { get; init; } = [];

    /// <summary>The tracks to play, interleaved with ad placeholders.</summary>
    public IReadOnlyList<TrackWithAds> TracksToPlayWithAds { get; init; } = [];

    /// <summary>The tracks to play for this day.</summary>
    public IReadOnlyList<Track> TracksToPlay { get; init; } = [];
}

/// <summary>A feed event, such as a new release, a recommendation, or an editorial message.</summary>
public sealed class Event
{
    /// <summary>The event identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The event type.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The type used when building "from" attribution strings, when present.</summary>
    public string? TypeForFrom { get; init; }

    /// <summary>The event title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The tracks associated with the event.</summary>
    public IReadOnlyList<Track> Tracks { get; init; } = [];

    /// <summary>The artists associated with the event.</summary>
    public IReadOnlyList<ArtistEvent> Artists { get; init; } = [];

    /// <summary>The albums associated with the event.</summary>
    public IReadOnlyList<AlbumEvent> Albums { get; init; } = [];

    /// <summary>The editorial message, when present.</summary>
    public string? Message { get; init; }

    /// <summary>The device the event relates to, when present.</summary>
    public string? Device { get; init; }

    /// <summary>The total track count, when present.</summary>
    public int? TracksCount { get; init; }

    /// <summary>The genre the event relates to, when present.</summary>
    public string? Genre { get; init; }
}

/// <summary>A track entry paired with its ad-eligibility type within a feed day.</summary>
public sealed class TrackWithAds
{
    /// <summary>The entry type (for example a track or an ad placeholder).</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The track, when this entry carries one.</summary>
    public Track? Track { get; init; }
}

/// <summary>An album surfaced inside a feed event, together with its representative tracks.</summary>
public sealed class AlbumEvent
{
    /// <summary>The album, when present.</summary>
    public Album? Album { get; init; }

    /// <summary>The representative tracks for the album.</summary>
    public IReadOnlyList<Track> Tracks { get; init; } = [];
}

/// <summary>An artist surfaced inside a feed event, together with related tracks and similarity hints.</summary>
public sealed class ArtistEvent
{
    /// <summary>The artist, when present.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The representative tracks for the artist.</summary>
    public IReadOnlyList<Track> Tracks { get; init; } = [];

    /// <summary>Artists from the listening history this artist is similar to.</summary>
    public IReadOnlyList<Artist> SimilarToArtistsFromHistory { get; init; } = [];

    /// <summary>Whether the user is subscribed to the artist, when known.</summary>
    public bool? Subscribed { get; init; }
}
