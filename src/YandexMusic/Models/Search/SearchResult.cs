using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Models.Playlists;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Search;

/// <summary>
/// The result of a catalogue search: the single best match plus a section per content category.
/// Sections are <see langword="null"/> when the query matched nothing in that category or the
/// search was restricted to a different type.
/// </summary>
public sealed class SearchResult
{
    /// <summary>The identifier of this search request, used for analytics and follow-up calls.</summary>
    public string? SearchRequestId { get; init; }

    /// <summary>The query text the results correspond to.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Whether automatic misspell correction was disabled for this request.</summary>
    public bool Nocorrect { get; init; }

    /// <summary>Whether the query was automatically corrected for a misspelling.</summary>
    public bool? MisspellCorrected { get; init; }

    /// <summary>The corrected query text, when the original was misspelled.</summary>
    public string? MisspellResult { get; init; }

    /// <summary>The original query text, when it was corrected.</summary>
    public string? MisspellOriginal { get; init; }

    /// <summary>The single best match for the query, across all categories.</summary>
    public SearchBest? Best { get; init; }

    /// <summary>The matching tracks.</summary>
    public SearchSection<Track>? Tracks { get; init; }

    /// <summary>The matching albums.</summary>
    public SearchSection<Album>? Albums { get; init; }

    /// <summary>The matching artists.</summary>
    public SearchSection<Artist>? Artists { get; init; }

    /// <summary>The matching playlists.</summary>
    public SearchSection<Playlist>? Playlists { get; init; }

    /// <summary>The matching podcasts (modelled as albums with a podcast meta-type).</summary>
    public SearchSection<Album>? Podcasts { get; init; }

    /// <summary>The matching podcast episodes (modelled as tracks).</summary>
    [JsonPropertyName("podcast_episodes")]
    public SearchSection<Track>? PodcastEpisodes { get; init; }

    /// <summary>The matching users.</summary>
    public SearchSection<User>? Users { get; init; }
}
