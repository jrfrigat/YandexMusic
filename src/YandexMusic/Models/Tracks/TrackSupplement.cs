using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>The lyrics of a track.</summary>
public sealed class Lyrics
{
    /// <summary>
    /// The lyrics identifier. Kept as text because the API returns it as a number on some responses
    /// and as a string on others, and nothing is ever computed from it.
    /// </summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; init; }

    /// <summary>The lyrics text (may be truncated when rights are limited).</summary>
    [JsonPropertyName("lyrics")]
    public string? Text { get; init; }

    /// <summary>The full lyrics text, when available.</summary>
    [JsonPropertyName("fullLyrics")]
    public string? FullText { get; init; }

    /// <summary>Whether the full lyrics may be displayed.</summary>
    public bool HasRights { get; init; }

    /// <summary>The language of the lyrics text.</summary>
    public string? TextLanguage { get; init; }

    /// <summary>Whether a translation is offered.</summary>
    public bool ShowTranslation { get; init; }
}

/// <summary>Supplementary information for a track, such as its lyrics.</summary>
public sealed class TrackSupplement
{
    /// <summary>
    /// The supplement identifier (matches the track). Kept as text: the API answers with a string
    /// here and a number elsewhere, and user-uploaded tracks carry ids that are not numbers at all.
    /// </summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; init; }

    /// <summary>The track lyrics, when available.</summary>
    public Lyrics? Lyrics { get; init; }
}
