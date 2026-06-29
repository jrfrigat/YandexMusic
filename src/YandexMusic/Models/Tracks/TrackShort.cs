using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>
/// A track as it appears inside a playlist or library: a lightweight reference that carries the
/// playlist-specific metadata and, when the endpoint includes it, the full <see cref="Track"/>.
/// </summary>
public sealed class TrackShort
{
    /// <summary>
    /// The track identifier. The API may send this as a number or a string, so it is normalized to a
    /// string to match <see cref="Tracks.Track.Id"/>.
    /// </summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Id { get; init; } = string.Empty;

    /// <summary>The album identifier the track is referenced from, when provided.</summary>
    public string? AlbumId { get; init; }

    /// <summary>The original position of the track within its container.</summary>
    public int OriginalIndex { get; init; }

    /// <summary>When the track was added to the container, when known.</summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>The full track, when the endpoint embeds it; otherwise <see langword="null"/>.</summary>
    public Track? Track { get; init; }
}
