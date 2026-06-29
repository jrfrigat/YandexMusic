using System.Text.Json.Serialization;
using YandexMusic.Models.Tracks;
using YandexMusic.Serialization;

namespace YandexMusic.Models.Playlists;

/// <summary>Who can see a playlist.</summary>
[JsonConverter(typeof(TolerantEnumConverter<PlaylistVisibility>))]
public enum PlaylistVisibility
{
    /// <summary>An unrecognised visibility.</summary>
    Unknown = 0,

    /// <summary>Visible to everyone.</summary>
    Public,

    /// <summary>Visible only to the owner.</summary>
    Private,
}

/// <summary>A genre or mood tag attached to a playlist.</summary>
public sealed class PlaylistTag
{
    /// <summary>The tag identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The human-readable tag value.</summary>
    public string Value { get; init; } = string.Empty;
}

/// <summary>A playlist of tracks.</summary>
public sealed class Playlist
{
    /// <summary>The playlist owner.</summary>
    public User Owner { get; init; } = new();

    /// <summary>The globally-unique playlist identifier.</summary>
    public string? PlaylistUuid { get; init; }

    /// <summary>The owner's user identifier.</summary>
    public long Uid { get; init; }

    /// <summary>The playlist kind, unique per owner. Together with <see cref="Uid"/> it identifies the playlist.</summary>
    public long Kind { get; init; }

    /// <summary>The playlist title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The plain-text description, when present.</summary>
    public string? Description { get; init; }

    /// <summary>The formatted (HTML) description, when present.</summary>
    public string? DescriptionFormatted { get; init; }

    /// <summary>The current revision number, incremented on every change.</summary>
    public int Revision { get; init; }

    /// <summary>The snapshot number used for optimistic concurrency when modifying the playlist.</summary>
    public int Snapshot { get; init; }

    /// <summary>The number of tracks in the playlist.</summary>
    public int TrackCount { get; init; }

    /// <summary>Who can see the playlist.</summary>
    public PlaylistVisibility Visibility { get; init; }

    /// <summary>Whether the playlist can be edited collaboratively.</summary>
    public bool Collective { get; init; }

    /// <summary>Whether the playlist is available in the current region.</summary>
    public bool Available { get; init; }

    /// <summary>When the playlist was created, when known.</summary>
    public DateTimeOffset? Created { get; init; }

    /// <summary>When the playlist was last modified, when known.</summary>
    public DateTimeOffset? Modified { get; init; }

    /// <summary>The total duration of the playlist, in milliseconds.</summary>
    public long DurationMs { get; init; }

    /// <summary>The number of users who liked the playlist.</summary>
    public int LikesCount { get; init; }

    /// <summary>The structured cover image, when available.</summary>
    public Cover? Cover { get; init; }

    /// <summary>The Open Graph image URI template, when available.</summary>
    public string? OgImage { get; init; }

    /// <summary>The tracks, each wrapped with its playlist metadata. Empty when not requested.</summary>
    public IReadOnlyList<TrackShort> Tracks { get; init; } = [];

    /// <summary>Pagination information, when the tracks were paged.</summary>
    public Pager? Pager { get; init; }

    /// <summary>The genre/mood tags attached to the playlist, when available.</summary>
    public IReadOnlyList<PlaylistTag>? Tags { get; init; }

    /// <summary>The kind of auto-generated playlist (for example the "playlist of the day"), when applicable.</summary>
    public string? GeneratedPlaylistType { get; init; }

    /// <summary>The analytics context identifier the playlist was opened from, when provided.</summary>
    public string? IdForFrom { get; init; }
}
