using System.Text.Json.Serialization;
using YandexMusic.Models.Albums;
using YandexMusic.Models.Artists;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Tracks;

/// <summary>The kind of content a <see cref="Track"/> represents.</summary>
[JsonConverter(typeof(TolerantEnumConverter<TrackType>))]
public enum TrackType
{
    /// <summary>An unrecognised track type.</summary>
    Unknown = 0,

    /// <summary>A music track.</summary>
    Music,

    /// <summary>A podcast episode.</summary>
    PodcastEpisode,

    /// <summary>An audiobook chapter.</summary>
    AudioBook,
}

/// <summary>Controls how a track may be shared.</summary>
[JsonConverter(typeof(TolerantEnumConverter<TrackSharingFlag>))]
public enum TrackSharingFlag
{
    /// <summary>An unrecognised sharing flag.</summary>
    Unknown = 0,

    /// <summary>Sharing is allowed.</summary>
    Default,

    /// <summary>Only the cover may be shared.</summary>
    CoverOnly,

    /// <summary>Sharing a video is allowed.</summary>
    VideoAllowed,
}

/// <summary>The origin of a track within the catalogue.</summary>
[JsonConverter(typeof(TolerantEnumConverter<TrackSource>))]
public enum TrackSource
{
    /// <summary>An unrecognised source.</summary>
    Unknown = 0,

    /// <summary>A first-party Yandex Music track.</summary>
    Own,
}

/// <summary>The label/major a track or album belongs to.</summary>
public sealed class Major
{
    /// <summary>The major identifier.</summary>
    public long Id { get; init; }

    /// <summary>The major name.</summary>
    public string Name { get; init; } = string.Empty;
}

/// <summary>EBU R128 loudness measurements for a track.</summary>
public sealed class R128
{
    /// <summary>The integrated loudness, in LUFS.</summary>
    public double I { get; init; }

    /// <summary>The true peak, in dBTP.</summary>
    public double Tp { get; init; }
}

/// <summary>The fade-in/fade-out envelope of a track, in seconds.</summary>
public sealed class Fade
{
    /// <summary>The time the fade-in starts.</summary>
    public double InStart { get; init; }

    /// <summary>The time the fade-in finishes.</summary>
    public double InStop { get; init; }

    /// <summary>The time the fade-out starts.</summary>
    public double OutStart { get; init; }

    /// <summary>The time the fade-out finishes.</summary>
    public double OutStop { get; init; }
}

/// <summary>Indicates which kinds of lyrics are available for a track.</summary>
public sealed class LyricsInfo
{
    /// <summary>Whether time-synchronised lyrics are available.</summary>
    public bool HasAvailableSyncLyrics { get; init; }

    /// <summary>Whether plain text lyrics are available.</summary>
    public bool HasAvailableTextLyrics { get; init; }
}

/// <summary>A track in the Yandex Music catalogue.</summary>
public sealed class Track
{
    /// <summary>
    /// The track identifier (a string such as <c>"4"</c>). The API may send it as a number — for
    /// example in search results — so it is normalized to a string.
    /// </summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string Id { get; init; } = string.Empty;

    /// <summary>The canonical track identifier, which may differ from <see cref="Id"/> for aliases.</summary>
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? RealId { get; init; }

    /// <summary>The track title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The kind of content the track represents.</summary>
    public TrackType Type { get; init; }

    /// <summary>The label/major the track belongs to, when present.</summary>
    public Major? Major { get; init; }

    /// <summary>Whether the track is available in the current region.</summary>
    public bool Available { get; init; }

    /// <summary>Whether the track is available to Yandex Plus subscribers.</summary>
    public bool AvailableForPremiumUsers { get; init; }

    /// <summary>Whether the full track is available without a subscription.</summary>
    public bool AvailableFullWithoutPermission { get; init; }

    /// <summary>The option packages the track is available under (for example <c>"bookmate"</c>).</summary>
    public IReadOnlyList<string> AvailableForOptions { get; init; } = [];

    /// <summary>The track duration, in milliseconds.</summary>
    public long DurationMs { get; init; }

    /// <summary>The preview duration, in milliseconds.</summary>
    public int PreviewDurationMs { get; init; }

    /// <summary>The file size in bytes, when known (0 until download metadata is fetched).</summary>
    public long FileSize { get; init; }

    /// <summary>The artists credited on the track.</summary>
    public IReadOnlyList<Artist> Artists { get; init; } = [];

    /// <summary>The albums the track appears on.</summary>
    public IReadOnlyList<Album> Albums { get; init; } = [];

    /// <summary>The cover URI template (with a <c>%%</c> size placeholder), when available.</summary>
    public string? CoverUri { get; init; }

    /// <summary>The Open Graph image URI template, when available.</summary>
    public string? OgImage { get; init; }

    /// <summary>The accent colours derived from the cover art, when available.</summary>
    public DerivedColors? DerivedColors { get; init; }

    /// <summary>Whether lyrics are available for the track.</summary>
    public bool LyricsAvailable { get; init; }

    /// <summary>Which kinds of lyrics are available, when known.</summary>
    public LyricsInfo? LyricsInfo { get; init; }

    /// <summary>Whether the player should remember the playback position (used by podcasts/audiobooks).</summary>
    public bool RememberPosition { get; init; }

    /// <summary>Controls how the track may be shared.</summary>
    public TrackSharingFlag TrackSharingFlag { get; init; }

    /// <summary>The origin of the track within the catalogue.</summary>
    public TrackSource TrackSource { get; init; }

    /// <summary>The EBU R128 loudness measurements, when available.</summary>
    public R128? R128 { get; init; }

    /// <summary>The fade-in/fade-out envelope, when available.</summary>
    public Fade? Fade { get; init; }

    /// <summary>A qualifier shown next to the title (for example <c>"feat. X"</c> or a remix name), when present.</summary>
    public string? Version { get; init; }

    /// <summary>A content warning such as <c>explicit</c>, when present.</summary>
    public string? ContentWarning { get; init; }

    /// <summary>A short description (used by podcast episodes and audiobooks), when present.</summary>
    public string? ShortDescription { get; init; }

    /// <summary>A background video URI template (used by some podcast episodes), when present.</summary>
    public string? BackgroundVideoUri { get; init; }

    /// <summary>Whether the track is marked as one of its album's highlights.</summary>
    public bool Best { get; init; }

    /// <summary>The internal storage directory of the track, when present.</summary>
    public string? StorageDir { get; init; }

    /// <summary>The replacement track served when the original is unavailable in the region, when present.</summary>
    public Track? Substituted { get; init; }
}
