using System.Text.Json.Serialization;

namespace YandexMusic.Ynison;

/// <summary>The kind of device participating in a Ynison session, as reported by the device itself.</summary>
[JsonConverter(typeof(ProtoEnumConverter<DeviceType>))]
public enum DeviceType
{
    /// <summary>Not specified.</summary>
    Unspecified = 0,

    /// <summary>A browser.</summary>
    Web = 1,

    /// <summary>An Android phone or tablet.</summary>
    Android = 2,

    /// <summary>An iOS phone or tablet.</summary>
    Ios = 3,

    /// <summary>A smart speaker.</summary>
    SmartSpeaker = 4,

    /// <summary>A web TV.</summary>
    WebTv = 5,

    /// <summary>An Android TV.</summary>
    AndroidTv = 6,

    /// <summary>An Apple TV.</summary>
    AppleTv = 7,

    /// <summary>An Android watch.</summary>
    AndroidWear = 8,

    /// <summary>The web desktop app for Windows.</summary>
    WebDesktop = 9,
}

/// <summary>The kind of entity a <see cref="Playable"/> represents.</summary>
[JsonConverter(typeof(ProtoEnumConverter<PlayableType>))]
public enum PlayableType
{
    /// <summary>Not specified.</summary>
    Unspecified = 0,

    /// <summary>A regular catalogue track.</summary>
    Track = 1,

    /// <summary>A track stored locally on the device.</summary>
    LocalTrack = 2,

    /// <summary>An infinite stream (radio or generative music).</summary>
    Infinite = 3,

    /// <summary>A video clip.</summary>
    VideoClip = 4,
}

/// <summary>How a video clip got into the queue (for analytics).</summary>
[JsonConverter(typeof(ProtoEnumConverter<VideoClipRecommendationType>))]
public enum VideoClipRecommendationType
{
    /// <summary>Not specified.</summary>
    Unspecified = 0,

    /// <summary>Came from recommendations.</summary>
    Recommended = 1,

    /// <summary>Explicitly chosen by the user.</summary>
    OnDemand = 2,

    /// <summary>Chosen from search.</summary>
    Search = 3,

    /// <summary>Chosen from the artist screen.</summary>
    Artist = 4,

    /// <summary>Chosen in the user's own collection.</summary>
    Own = 5,

    /// <summary>Came in a recommendations block on the trends page.</summary>
    EditorialChoice = 6,
}

/// <summary>The entity a playback queue was built from (legacy; see <see cref="PlayerQueueQueue"/>).</summary>
[JsonConverter(typeof(ProtoEnumConverter<QueueEntityType>))]
public enum QueueEntityType
{
    /// <summary>Not specified.</summary>
    Unspecified = 0,

    /// <summary>The artist's popular tracks.</summary>
    Artist = 1,

    /// <summary>Tracks of a playlist.</summary>
    Playlist = 2,

    /// <summary>Tracks of an album.</summary>
    Album = 3,

    /// <summary>A dynamic radio queue seeded by a station.</summary>
    Radio = 4,

    /// <summary>An arbitrary set of tracks.</summary>
    Various = 5,

    /// <summary>A generative music stream.</summary>
    Generative = 6,

    /// <summary>An FM radio stream.</summary>
    FmRadio = 7,

    /// <summary>A dynamic queue of video clips.</summary>
    VideoWave = 8,

    /// <summary>A queue of local device tracks.</summary>
    LocalTracks = 9,
}

/// <summary>The playback context of a queue.</summary>
[JsonConverter(typeof(ProtoEnumConverter<QueueEntityContext>))]
public enum QueueEntityContext
{
    /// <summary>The context is chosen by default depending on the entity.</summary>
    BasedOnEntityByDefault = 0,

    /// <summary>My tracks.</summary>
    UserTracks = 1,

    /// <summary>Downloaded tracks.</summary>
    DownloadedTracks = 2,

    /// <summary>From search.</summary>
    Search = 3,

    /// <summary>Listening history.</summary>
    MusicHistory = 4,

    /// <summary>Search over the listening history.</summary>
    MusicHistorySearch = 5,

    /// <summary>The artist's collection of the user.</summary>
    ArtistMyCollection = 6,

    /// <summary>Artists familiar from the wave.</summary>
    ArtistFamiliarFromWave = 7,
}

/// <summary>How a track got into the wave queue.</summary>
[JsonConverter(typeof(ProtoEnumConverter<WaveSourceSourceType>))]
public enum WaveSourceSourceType
{
    /// <summary>The track came from online recommendations.</summary>
    OnlineByDefault = 0,

    /// <summary>The track came from offline recommendations (offline wave).</summary>
    Offline = 1,
}

/// <summary>The repeat mode of playback.</summary>
[JsonConverter(typeof(ProtoEnumConverter<RepeatMode>))]
public enum RepeatMode
{
    /// <summary>Not specified.</summary>
    Unspecified = 0,

    /// <summary>No repeat.</summary>
    None = 1,

    /// <summary>Repeat the current track.</summary>
    One = 2,

    /// <summary>Repeat the whole queue.</summary>
    All = 3,
}

/// <summary>The kind of an entity injected into a queue (an Alice shot, an ad, and so on).</summary>
[JsonConverter(typeof(ProtoEnumConverter<InjectPlayableType>))]
public enum InjectPlayableType
{
    /// <summary>Not specified.</summary>
    Unspecified = 0,

    /// <summary>An Alice voice shot.</summary>
    AliceShot = 1,

    /// <summary>An ad block.</summary>
    Ad = 2,

    /// <summary>A preroll.</summary>
    Preroll = 3,
}

/// <summary>The activity-interception tactic of the device sending a state update.</summary>
[JsonConverter(typeof(ProtoEnumConverter<ActivityInterceptionType>))]
public enum ActivityInterceptionType
{
    /// <summary>The device does not try to become the active one.</summary>
    DoNotInterceptByDefault = 0,

    /// <summary>The device becomes active when no other device is active at handling time.</summary>
    InterceptIfNoOneActive = 1,

    /// <summary>The device receives activity after the message is successfully processed.</summary>
    InterceptEager = 2,
}
