using System.Text.Json.Serialization;
using YandexMusic.Serialization;

namespace YandexMusic.Ynison;

/// <summary>
/// The version of a state change: which device made it, a random monotonic-ish number, and when.
/// Servers and clients use it only to tell updates apart, not to order them.
/// </summary>
/// <param name="DeviceId">The identifier of the device that made the change.</param>
/// <param name="Version">A random <see cref="long"/> identifying this particular change.</param>
/// <param name="TimestampMs">Unix time of the change in milliseconds (diagnostic only).</param>
public sealed record UpdateVersion(
    string DeviceId,
    [property: JsonConverter(typeof(ProtoInt64Converter))] long Version,
    [property: JsonConverter(typeof(ProtoInt64Converter))] long TimestampMs);

/// <summary>Player-wide settings.</summary>
/// <param name="RepeatMode">The repeat mode.</param>
public sealed record PlayerStateOptions(RepeatMode RepeatMode);

/// <summary>Shuffle settings; present only while shuffle is enabled.</summary>
public sealed record Shuffle
{
    /// <summary>The shuffled indices of the entities in the queue.</summary>
    public IReadOnlyList<int> PlayableIndices { get; init; } = [];
}

/// <summary>The playback status of a queue.</summary>
/// <param name="ProgressMs">The progress of the played entity in milliseconds; always 0 for infinite queues.</param>
/// <param name="DurationMs">The duration of the played entity in milliseconds; always 0 for infinite queues.</param>
/// <param name="Paused">Whether playback is paused.</param>
/// <param name="PlaybackSpeed">The playback speed multiplier.</param>
/// <param name="Version">The version of the last status change.</param>
public sealed record PlayingStatus(
    [property: JsonConverter(typeof(ProtoInt64Converter))] long ProgressMs,
    [property: JsonConverter(typeof(ProtoInt64Converter))] long DurationMs,
    bool Paused,
    double PlaybackSpeed,
    UpdateVersion? Version = null);

/// <summary>The state of an injected playable (an Alice shot, a preroll) playing alongside the queue.</summary>
/// <param name="PlayingStatus">The playback status of the injected entity.</param>
/// <param name="Playable">The injected playable.</param>
/// <param name="Version">The version of the last change of this state.</param>
public sealed record PlayerQueueInject(
    PlayingStatus? PlayingStatus,
    PlayerQueueInjectPlayable? Playable,
    UpdateVersion? Version = null);

/// <summary>An entity injected into a queue: an Alice shot, an ad, a preroll.</summary>
/// <param name="PlayableId">The entity identifier.</param>
/// <param name="PlayableType">The entity type.</param>
/// <param name="Title">The title.</param>
/// <param name="CoverUrlOptional">An optional cover URL; may contain an avatar placeholder for the size.</param>
public sealed record PlayerQueueInjectPlayable(
    string PlayableId,
    InjectPlayableType PlayableType,
    string Title,
    string? CoverUrlOptional = null);

/// <summary>The full player state: what is playing, from which queue, and how.</summary>
/// <param name="Status">The playback status.</param>
/// <param name="PlayerQueue">The current queue.</param>
/// <param name="PlayerQueueInjectOptional">The state of an injected playable, when one is playing.</param>
public sealed record PlayerState(
    PlayingStatus? Status,
    PlayerQueue? PlayerQueue,
    PlayerQueueInject? PlayerQueueInjectOptional = null);

/// <summary>Additional track information carried by a playable.</summary>
/// <param name="TrackSourceKey">The source key from the wave queue's track-source table.</param>
/// <param name="BatchIdOptional">The recommendation batch id; required for wave-queue tracks.</param>
public sealed record TrackInfo(int TrackSourceKey, string? BatchIdOptional = null);

/// <summary>Additional video-clip information carried by a playable.</summary>
/// <param name="RecommendationType">How the clip got into the queue (for analytics).</param>
public sealed record VideoClipInfo(VideoClipRecommendationType RecommendationType);

/// <summary>
/// A single playable entity: a track, a video clip, and so on. The composite id for
/// <see cref="PlayableType.Track"/> is built from <paramref name="PlayableId"/> and
/// <paramref name="AlbumIdOptional"/>.
/// </summary>
/// <param name="PlayableId">The entity identifier.</param>
/// <param name="AlbumIdOptional">The optional album id of a track.</param>
/// <param name="PlayableType">The entity type.</param>
/// <param name="From">The analytics "from" for play-audio.</param>
/// <param name="Title">The title.</param>
/// <param name="CoverUrlOptional">An optional cover URL; may contain an avatar placeholder for the size.</param>
/// <param name="VideoClipInfo">Clip details; set only for <see cref="PlayableType.VideoClip"/>.</param>
/// <param name="TrackInfo">Track details; set only for <see cref="PlayableType.Track"/>.</param>
/// <param name="NavigationIdOptional">A hash for play-audio navigation analytics.</param>
/// <param name="PlaybackActionIdOptional">An id gluing a playback to the screen it started from (analytics).</param>
public sealed record Playable(
    string PlayableId,
    string? AlbumIdOptional,
    PlayableType PlayableType,
    string From,
    string Title,
    string? CoverUrlOptional = null,
    VideoClipInfo? VideoClipInfo = null,
    TrackInfo? TrackInfo = null,
    string? NavigationIdOptional = null,
    string? PlaybackActionIdOptional = null);
