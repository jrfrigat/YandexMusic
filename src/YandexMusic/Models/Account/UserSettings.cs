namespace YandexMusic.Models.Account;

/// <summary>The signed-in user's playback and privacy preferences.</summary>
public sealed class UserSettings
{
    /// <summary>The user identifier.</summary>
    public long Uid { get; init; }

    /// <summary>Whether scrobbling to Last.fm is enabled.</summary>
    public bool LastFmScrobblingEnabled { get; init; }

    /// <summary>Whether shuffle is enabled.</summary>
    public bool ShuffleEnabled { get; init; }

    /// <summary>The default playback volume, as a percentage.</summary>
    public int VolumePercents { get; init; }

    /// <summary>When the settings were last modified.</summary>
    public string? Modified { get; init; }

    /// <summary>Whether scrobbling to Facebook is enabled.</summary>
    public bool FacebookScrobblingEnabled { get; init; }

    /// <summary>Whether newly-liked tracks are added to the top of the playlist.</summary>
    public bool AddNewTrackOnPlaylistTop { get; init; }

    /// <summary>The visibility of the user's music. Known values: <c>private</c>, <c>public</c>.</summary>
    public string? UserMusicVisibility { get; init; }

    /// <summary>The visibility of the user's social activity. Known values: <c>private</c>, <c>public</c>.</summary>
    public string? UserSocialVisibility { get; init; }

    /// <summary>Whether ring-back-tone integration is disabled.</summary>
    public bool RbtDisabled { get; init; }

    /// <summary>The UI theme. Known values: <c>white</c>, <c>black</c>.</summary>
    public string? Theme { get; init; }

    /// <summary>Whether promotional content is disabled.</summary>
    public bool PromosDisabled { get; init; }

    /// <summary>Whether radio auto-play is enabled.</summary>
    public bool AutoPlayRadio { get; init; }

    /// <summary>Whether queue synchronization across devices is enabled.</summary>
    public bool SyncQueueEnabled { get; init; }

    /// <summary>Whether advertisements are disabled.</summary>
    public bool AdsDisabled { get; init; }

    /// <summary>Whether Yandex Disk integration is enabled, when reported.</summary>
    public bool? DiskEnabled { get; init; }

    /// <summary>Whether tracks stored on Yandex Disk are shown in the library, when reported.</summary>
    public bool? ShowDiskTracksInLibrary { get; init; }
}
