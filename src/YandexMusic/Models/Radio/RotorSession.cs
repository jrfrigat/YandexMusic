using System.Text.Json.Serialization;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Radio;

/// <summary>
/// A radio session: the current way to listen to radio. A session is opened from a set of seeds,
/// hands out batches of tracks and takes feedback about what was played, which replaces the older
/// per-station <c>tracks</c>/<c>feedback</c> pair. Its <see cref="RadioSessionId"/> identifies it for
/// every follow-up call.
/// </summary>
public sealed class RotorSession
{
    /// <summary>The session identifier used to fetch further tracks and to report feedback.</summary>
    [JsonPropertyName("radioSessionId")]
    public string? RadioSessionId { get; init; }

    /// <summary>The identifier of the batch the tracks below belong to.</summary>
    [JsonPropertyName("batchId")]
    public string? BatchId { get; init; }

    /// <summary>Whether the seasonal "pumpkin" presentation is active.</summary>
    public bool? Pumpkin { get; init; }

    /// <summary>The first batch of tracks, unless the request asked for them to be left out.</summary>
    public IReadOnlyList<Sequence>? Sequence { get; init; }

    /// <summary>The seeds the server accepted; it may drop or rewrite the ones it was given.</summary>
    [JsonPropertyName("acceptedSeeds")]
    public IReadOnlyList<RotorSeed>? AcceptedSeeds { get; init; }

    /// <summary>The seed that describes the session as a whole.</summary>
    [JsonPropertyName("descriptionSeed")]
    public RotorSeed? DescriptionSeed { get; init; }

    /// <summary>Whether the session has ended and will serve no further tracks.</summary>
    public bool? Terminated { get; init; }

    /// <summary>The wave behind the session. Only returned when the request asked for the wave model.</summary>
    public Wave? Wave { get; init; }

    /// <summary>The data an offline recommender needs to keep playing without the network.</summary>
    [JsonPropertyName("offlineRecommenderData")]
    public IReadOnlyList<int>? OfflineRecommenderData { get; init; }

    /// <summary>Whether the session is interactive (the listener may skip and tune it).</summary>
    public bool? Interactive { get; init; }
}

/// <summary>
/// The next batch of tracks of a radio session. A session that no longer exists is not an error:
/// the server answers with <see cref="UnknownSession"/> set instead.
/// </summary>
public sealed class RotorSessionTracks
{
    /// <summary>The identifier of this batch, echoed back in feedback events.</summary>
    [JsonPropertyName("batchId")]
    public string? BatchId { get; init; }

    /// <summary>Whether the seasonal "pumpkin" presentation is active.</summary>
    public bool? Pumpkin { get; init; }

    /// <summary>The tracks of the batch.</summary>
    public IReadOnlyList<Sequence>? Sequence { get; init; }

    /// <summary>Whether the session has ended and will serve no further tracks.</summary>
    public bool? Terminated { get; init; }

    /// <summary>Whether the session identifier is unknown to the server, so a new session is needed.</summary>
    [JsonPropertyName("unknownSession")]
    public bool? UnknownSession { get; init; }

    /// <summary>The data an offline recommender needs to keep playing without the network.</summary>
    [JsonPropertyName("offlineRecommenderData")]
    public IReadOnlyList<int>? OfflineRecommenderData { get; init; }
}

/// <summary>
/// A radio seed: what a session is built from. In string form it is written <c>type:tag</c>, for
/// example <c>user:onyourwave</c>, <c>genre:rock</c>, <c>track:12345</c> or a wave setting such as
/// <c>settingDiversity:discover</c>.
/// </summary>
public sealed class RotorSeed
{
    /// <summary>The seed type, for example <c>user</c>, <c>genre</c> or <c>track</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The seed tag scoped to its type.</summary>
    public string? Tag { get; init; }

    /// <summary>The seed value, when the server reports one separately from the tag.</summary>
    public string? Value { get; init; }

    /// <summary>Renders the seed in the <c>type:tag</c> form a session request expects.</summary>
    /// <returns>The serialized seed, or an empty string when the type is missing.</returns>
    public override string ToString()
        => string.IsNullOrEmpty(Type) ? string.Empty : $"{Type}:{Tag ?? Value ?? string.Empty}";
}

/// <summary>
/// Parameters a radio session reports for a track: the musical characteristics the player uses to
/// present it and to cross-fade into the next one.
/// </summary>
public sealed class TrackParameters
{
    /// <summary>The tempo, in beats per minute.</summary>
    public int? Bpm { get; init; }

    /// <summary>How energetic the track is.</summary>
    public double? Energy { get; init; }

    /// <summary>The hue associated with the track.</summary>
    public double? Hue { get; init; }

    /// <summary>The hue associated with the listener's collection.</summary>
    [JsonPropertyName("userCollectionHue")]
    public int? UserCollectionHue { get; init; }

    /// <summary>The fade envelope to use when mixing this track with the next one.</summary>
    [JsonPropertyName("mixFade")]
    public Fade? MixFade { get; init; }
}
