using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Search;

/// <summary>The kind of entity a search result refers to.</summary>
[JsonConverter(typeof(TolerantEnumConverter<SearchResultType>))]
public enum SearchResultType
{
    /// <summary>An unrecognised result type.</summary>
    Unknown = 0,

    /// <summary>A track.</summary>
    Track,

    /// <summary>An album.</summary>
    Album,

    /// <summary>An artist.</summary>
    Artist,

    /// <summary>A playlist.</summary>
    Playlist,

    /// <summary>A podcast.</summary>
    Podcast,

    /// <summary>A podcast episode.</summary>
    PodcastEpisode,

    /// <summary>A video.</summary>
    Video,

    /// <summary>A user.</summary>
    User,
}

/// <summary>
/// The single best match for a search query. The <see cref="Type"/> indicates which of the typed
/// properties is populated.
/// </summary>
[JsonConverter(typeof(SearchBestConverter))]
public sealed class SearchBest
{
    /// <summary>The kind of entity the best match refers to.</summary>
    public SearchResultType Type { get; init; }

    /// <summary>The best match when <see cref="Type"/> is <see cref="SearchResultType.Track"/>.</summary>
    public Track? Track { get; init; }

    /// <summary>The best match when <see cref="Type"/> is <see cref="SearchResultType.Album"/>.</summary>
    public Album? Album { get; init; }

    /// <summary>The best match when <see cref="Type"/> is <see cref="SearchResultType.Artist"/>.</summary>
    public Artist? Artist { get; init; }

    /// <summary>The best match when <see cref="Type"/> is <see cref="SearchResultType.Playlist"/>.</summary>
    public Playlist? Playlist { get; init; }
}
