using System.Text.Json.Serialization;

namespace YandexMusic.Models.Radio;

/// <summary>
/// One feedback event of a radio session. Which members the server requires depends on
/// <see cref="Type"/>: the track events need <see cref="TrackId"/>, the <c>playableItem*</c> events
/// need <see cref="Playable"/>, and the events that end playback of something need
/// <see cref="TotalPlayedSeconds"/>. <see cref="Timestamp"/> is always required.
/// </summary>
/// <remarks>
/// Like and dislike events only steer the recommendations; they do not change the user's collection.
/// </remarks>
public sealed class SessionEvent
{
    /// <summary>
    /// The event type. Known values: <c>radioStarted</c>, <c>trackStarted</c>,
    /// <c>trackFinished</c>, <c>skip</c>, <c>skipFailed</c>, <c>like</c>, <c>unlike</c>,
    /// <c>dislike</c>, <c>undislike</c>, <c>playableItemStarted</c>, <c>playableItemFinished</c>,
    /// <c>playableItemSkip</c>, <c>playableItemLike</c>, <c>playableItemUnlike</c>,
    /// <c>playableItemDislike</c>, <c>playableItemUndislike</c>, <c>combinedQueueStarted</c> and
    /// <c>ad</c>.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>The event time. Defaults to the moment the event object is created.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>The track the event refers to, as <c>id</c> or <c>id:albumId</c>.</summary>
    public string? TrackId { get; init; }

    /// <summary>How many seconds of the track were played before the event.</summary>
    public double? TotalPlayedSeconds { get; init; }

    /// <summary>The played object, for the <c>playableItem*</c> events.</summary>
    public SessionPlayable? Playable { get; init; }
}

/// <summary>
/// The object a <c>playableItem*</c> event refers to. A track carries its identifier in
/// <see cref="TrackId"/>, a clip in <see cref="Id"/>.
/// </summary>
public sealed class SessionPlayable
{
    /// <summary>The object type: <c>track</c> or <c>clip</c>.</summary>
    public required string Type { get; init; }

    /// <summary>The track identifier, when the type is <c>track</c>.</summary>
    public string? TrackId { get; init; }

    /// <summary>The clip identifier, when the type is <c>clip</c>.</summary>
    public string? Id { get; init; }
}

/// <summary>A feedback event together with the batch and playback source it belongs to.</summary>
public sealed class SessionFeedback
{
    /// <summary>The event itself.</summary>
    public required SessionEvent Event { get; init; }

    /// <summary>The batch the event refers to.</summary>
    public string? BatchId { get; init; }

    /// <summary>Where playback was started from, for example <c>radio-mobile-user-onyourwave</c>.</summary>
    public string? From { get; init; }
}

/// <summary>The feedback collected for one session, used when reporting several sessions at once.</summary>
public sealed class SessionFeedbacks
{
    /// <summary>The session the events belong to.</summary>
    public required string SessionId { get; init; }

    /// <summary>The events to report.</summary>
    public required IReadOnlyList<SessionFeedback> Feedbacks { get; init; }
}

/// <summary>An item of a combined session: a track or a clip, with the type that tells them apart.</summary>
public sealed class CombinedSessionQueueItem
{
    /// <summary>The item type. Known values: <c>CLIP</c> and <c>TRACK</c>.</summary>
    public required string Type { get; init; }

    /// <summary>The identifier of the clip or track.</summary>
    public required string Id { get; init; }
}

/// <summary>
/// A combined radio session, which mixes tracks and clips in one queue. Unlike a track session it
/// returns its items as a flat <see cref="Items"/> list whose entries carry their own type.
/// </summary>
public sealed class CombinedSession
{
    /// <summary>The session identifier. The server sends it only when the session is created.</summary>
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; init; }

    /// <summary>The identifier of the batch the items below belong to.</summary>
    [JsonPropertyName("batchId")]
    public string? BatchId { get; init; }

    /// <summary>Whether the seasonal "pumpkin" presentation is active.</summary>
    public bool? Pumpkin { get; init; }

    /// <summary>The items of the session.</summary>
    [JsonPropertyName("list")]
    public IReadOnlyList<CombinedSessionItem>? Items { get; init; }
}

/// <summary>The landing block of a combined session, for example the "clip time" promo.</summary>
public sealed class CombinedSessionLanding
{
    /// <summary>The block title.</summary>
    public string? Title { get; init; }

    /// <summary>The block description.</summary>
    public string? Description { get; init; }

    /// <summary>The call-to-action label.</summary>
    public string? Button { get; init; }

    /// <summary>The items shown in the block.</summary>
    [JsonPropertyName("list")]
    public IReadOnlyList<CombinedSessionItem>? Items { get; init; }
}
