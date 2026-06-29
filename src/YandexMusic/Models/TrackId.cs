using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Models;

/// <summary>A lightweight reference to a track inside a queue or landing block.</summary>
public sealed class TrackId
{
    /// <summary>The track identifier, as referenced outside a queue.</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Id { get; init; }

    /// <summary>The track identifier as referenced inside a queue.</summary>
    [JsonPropertyName("trackId")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? TrackNumber { get; init; }

    /// <summary>The album the track is referenced from, when provided.</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? AlbumId { get; init; }

    /// <summary>The source the reference came from, when provided.</summary>
    [JsonPropertyName("from")]
    public string? From { get; init; }
}
