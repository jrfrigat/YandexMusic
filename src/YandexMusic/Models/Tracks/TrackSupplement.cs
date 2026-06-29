using System.Text.Json.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>The lyrics of a track.</summary>
public sealed class Lyrics
{
    /// <summary>The lyrics identifier.</summary>
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
    /// <summary>The supplement identifier (matches the track).</summary>
    public long Id { get; init; }

    /// <summary>The track lyrics, when available.</summary>
    public Lyrics? Lyrics { get; init; }
}
