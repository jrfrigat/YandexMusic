using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>The lyrics of a track.</summary>
public sealed class Lyrics
{
    /// <summary>The lyrics identifier. The API returns it as either a number or a string.</summary>
    [JsonConverter(typeof(ProtoInt64Converter))]
    public long Id { get; init; }

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
    /// <summary>The supplement identifier (matches the track). The API returns it as a string.</summary>
    [JsonConverter(typeof(ProtoInt64Converter))]
    public long Id { get; init; }

    /// <summary>The track lyrics, when available.</summary>
    public Lyrics? Lyrics { get; init; }
}
