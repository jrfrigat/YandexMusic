namespace YandexMusic.Ynison;

/// <summary>The hierarchical queue structure; exactly one queue kind is set.</summary>
/// <param name="WaveQueue">The wave (recommendations) queue.</param>
/// <param name="GenerativeQueue">The generative music stream.</param>
/// <param name="FmRadioQueue">The FM radio stream.</param>
/// <param name="VideoWaveQueue">The video-clip queue.</param>
/// <param name="LocalTracksQueue">The queue of local device tracks.</param>
public sealed record PlayerQueueQueue(
    WaveQueue? WaveQueue = null,
    GenerativeQueue? GenerativeQueue = null,
    FmRadioQueue? FmRadioQueue = null,
    VideoWaveQueue? VideoWaveQueue = null,
    LocalTracksQueue? LocalTracksQueue = null);

/// <summary>A rotor session backing a wave queue; when set, the queue is extended from it indefinitely.</summary>
/// <param name="SessionId">The rotor session identifier.</param>
public sealed record WaveSession(string SessionId);

/// <summary>A wave-source entry of the track-source table.</summary>
/// <param name="SourceType">How tracks from this source got into the wave.</param>
public sealed record WaveSource(WaveSourceSourceType SourceType);

/// <summary>An artist id in a phonoteka source (string for historical reasons).</summary>
/// <param name="Id">The artist identifier.</param>
public sealed record PhonotekaArtistId(string Id);

/// <summary>A playlist id in a phonoteka source (string because of the owner:kind deprecation).</summary>
/// <param name="Id">The playlist identifier.</param>
/// <param name="FilterOptional">The filter id the tracks were started from, when not all tracks are played.</param>
public sealed record PhonotekaPlaylistId(string Id, string? FilterOptional = null);

/// <summary>An album id in a phonoteka source (string for historical reasons).</summary>
/// <param name="Id">The album identifier.</param>
public sealed record PhonotekaAlbumId(string Id);

/// <summary>A phonoteka (library entity) source of tracks in a wave queue.</summary>
/// <param name="EntityContext">The playback context.</param>
/// <param name="ArtistId">The artist the tracks came from; one of the entity ids.</param>
/// <param name="PlaylistId">The playlist the tracks came from; one of the entity ids.</param>
/// <param name="AlbumId">The album the tracks came from; one of the entity ids.</param>
public sealed record PhonotekaSource(
    QueueEntityContext EntityContext,
    PhonotekaArtistId? ArtistId = null,
    PhonotekaPlaylistId? PlaylistId = null,
    PhonotekaAlbumId? AlbumId = null);

/// <summary>One row of the wave queue's track-source table: a key plus the source it stands for.</summary>
/// <param name="Key">The key referenced from <see cref="TrackInfo.TrackSourceKey"/>; unique within the table.</param>
/// <param name="WaveSource">The wave source; one of the source kinds.</param>
/// <param name="PhonotekaSource">The phonoteka source; one of the source kinds.</param>
public sealed record TrackSourceWithKey(
    int Key,
    WaveSource? WaveSource = null,
    PhonotekaSource? PhonotekaSource = null);

/// <summary>Entity options of a wave queue.</summary>
public sealed record WaveQueueEntityOptions
{
    /// <summary>The rotor session of the recommended tracks, once requested.</summary>
    public WaveSession? WaveEntityOptional { get; init; }

    /// <summary>The entity/context table compressing the queue's state.</summary>
    public IReadOnlyList<TrackSourceWithKey> TrackSources { get; init; } = [];
}

/// <summary>
/// The wave queue: listened tracks plus queued and recommended tracks, extended automatically
/// while <see cref="WaveQueueEntityOptions.WaveEntityOptional"/> is set.
/// </summary>
public sealed record WaveQueue
{
    /// <summary>The not-yet-listened recommended tracks.</summary>
    public IReadOnlyList<Playable> RecommendedPlayableList { get; init; } = [];

    /// <summary>The index of the last listened track (or the potential recommendation slot).</summary>
    public int LivePlayableIndex { get; init; }

    /// <summary>The entity options table.</summary>
    public WaveQueueEntityOptions? EntityOptions { get; init; }

    /// <summary>A stable analytics hash of the queue.</summary>
    public string? NavigationIdOptional { get; init; }

    /// <summary>An id gluing playbacks to the screen the queue started from.</summary>
    public string? PlaybackActionIdOptional { get; init; }
}

/// <summary>A generative music stream consisting of a single infinite playable.</summary>
/// <param name="Id">The stream identifier.</param>
public sealed record GenerativeQueue(string Id);

/// <summary>An FM radio stream consisting of a single infinite playable.</summary>
/// <param name="Id">The stream identifier.</param>
public sealed record FmRadioQueue(string Id);

/// <summary>A recommendation queue of video clips.</summary>
/// <param name="Id">An arbitrary queue id: "default", "search:{clipId}", "{albumId}", and so on.</param>
public sealed record VideoWaveQueue(string Id);

/// <summary>A queue of local device tracks; it cannot be moved to another device via Ynison.</summary>
public sealed record LocalTracksQueue;

/// <summary>The entity a queue was initialized from (legacy).</summary>
/// <param name="EntityId">The entity identifier.</param>
/// <param name="EntityType">The entity type.</param>
public sealed record PlayerQueueInitialEntity(string EntityId, QueueEntityType EntityType);

/// <summary>Radio options of a queue (legacy).</summary>
/// <param name="SessionId">The rotor session identifier.</param>
public sealed record PlayerQueueRadioOptions(string SessionId);

/// <summary>Additional queue parameters (legacy).</summary>
/// <param name="RadioOptions">The radio options, when the queue is a radio queue.</param>
public sealed record PlayerQueuePlayerQueueOptions(PlayerQueueRadioOptions? RadioOptions = null);

/// <summary>
/// The playback queue. The legacy <paramref name="EntityId"/>/<paramref name="EntityType"/> pair
/// still arrives; new code should read <paramref name="Queue"/>.
/// </summary>
/// <param name="EntityId">The legacy entity id the queue was built from.</param>
/// <param name="EntityType">The legacy entity type.</param>
/// <param name="Queue">The hierarchical queue structure.</param>
/// <param name="CurrentPlayableIndex">The index of the current playable in <paramref name="PlayableList"/>.</param>
/// <param name="Options">The player settings.</param>
/// <param name="Version">The version of the last queue change.</param>
/// <param name="ShuffleOptional">The shuffle settings; absent while shuffle is off.</param>
/// <param name="EntityContext">The legacy playback context.</param>
/// <param name="FromOptional">The queue-level "from" for play-audio analytics.</param>
/// <param name="InitialEntityOptional">The legacy initial entity.</param>
/// <param name="AddingOptionsOptional">The legacy additional parameters.</param>
/// <param name="NavigationIdOptional">A backwards-compatible navigation hash for non-WaveQueue clients.</param>
/// <param name="FilterOptional">A backwards-compatible playlist filter for non-WaveQueue clients.</param>
/// <param name="PlaybackActionIdOptional">A backwards-compatible playback-action id for non-WaveQueue clients.</param>
public sealed record PlayerQueue(
    string EntityId,
    QueueEntityType EntityType,
    PlayerQueueQueue? Queue = null,
    int CurrentPlayableIndex = 0,
    PlayerStateOptions? Options = null,
    UpdateVersion? Version = null,
    Shuffle? ShuffleOptional = null,
    QueueEntityContext EntityContext = QueueEntityContext.BasedOnEntityByDefault,
    string? FromOptional = null,
    PlayerQueueInitialEntity? InitialEntityOptional = null,
    PlayerQueuePlayerQueueOptions? AddingOptionsOptional = null,
    string? NavigationIdOptional = null,
    string? FilterOptional = null,
    string? PlaybackActionIdOptional = null)
{
    /// <summary>The entities in the queue.</summary>
    public IReadOnlyList<Playable> PlayableList { get; init; } = [];
}
