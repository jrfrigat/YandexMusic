using System.Text.Json.Serialization;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Radio;

/// <summary>A batch of tracks streamed from a station, identified by a batch id.</summary>
public sealed class StationTracksResult
{
    /// <summary>The station identity the batch belongs to.</summary>
    public StationId? Id { get; init; }

    /// <summary>The ordered sequence of items in the batch.</summary>
    public IReadOnlyList<Sequence> Sequence { get; init; } = [];

    /// <summary>The batch identifier, echoed back when reporting feedback.</summary>
    [JsonPropertyName("batchId")]
    public string BatchId { get; init; } = string.Empty;

    /// <summary>Whether the seasonal "pumpkin" presentation is active.</summary>
    public bool Pumpkin { get; init; }

    /// <summary>
    /// The radio session the batch was served in, when the station is backed by a session. It is the
    /// identifier to use with the session endpoints rather than starting a new session.
    /// </summary>
    [JsonPropertyName("radioSessionId")]
    public string? RadioSessionId { get; init; }
}

/// <summary>A single item in a station batch: a track and whether it is already liked.</summary>
public sealed class Sequence
{
    /// <summary>The item type. The common value is <c>track</c>.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The track of the item, when the item is a track.</summary>
    public Track? Track { get; init; }

    /// <summary>Whether the current user already liked the track.</summary>
    public bool Liked { get; init; }

    /// <summary>The musical characteristics radio reports for the track, when it provides them.</summary>
    [JsonPropertyName("trackParameters")]
    public TrackParameters? TrackParameters { get; init; }
}
