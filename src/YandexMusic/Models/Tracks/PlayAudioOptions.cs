namespace YandexMusic.Models.Tracks;

/// <summary>The playback report sent to the play-audio endpoint to track listening progress.</summary>
public sealed class PlayAudioOptions
{
    /// <summary>The identifier of the track being played.</summary>
    public required string TrackId { get; init; }

    /// <summary>The name of the client the playback originates from.</summary>
    public required string From { get; init; }

    /// <summary>The identifier of the album the track is played from.</summary>
    public required string AlbumId { get; init; }

    /// <summary>The identifier of the playlist the track is played from, when applicable.</summary>
    public string? PlaylistId { get; init; }

    /// <summary>Whether the track is being played from a local cache.</summary>
    public bool FromCache { get; init; }

    /// <summary>A unique identifier for this playback session.</summary>
    public string? PlayId { get; init; }

    /// <summary>The listening account identifier. Defaults to the signed-in account when omitted.</summary>
    public long? Uid { get; init; }

    /// <summary>The moment playback started. Defaults to now when omitted.</summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>The total length of the track, in seconds.</summary>
    public int TrackLengthSeconds { get; init; }

    /// <summary>The number of seconds of the track played so far.</summary>
    public int TotalPlayedSeconds { get; init; }

    /// <summary>The final played position, in seconds.</summary>
    public int EndPositionSeconds { get; init; }

    /// <summary>The current client time. Defaults to now when omitted.</summary>
    public DateTimeOffset? ClientNow { get; init; }
}
