using YandexMusic.Models.Albums;
using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Library;

/// <summary>A user's liked tracks, with the revision used for optimistic concurrency.</summary>
public sealed class LikedTracks
{
    /// <summary>The owner's user identifier.</summary>
    public long Uid { get; init; }

    /// <summary>The revision number, incremented on every change to the library.</summary>
    public int Revision { get; init; }

    /// <summary>
    /// The liked tracks as lightweight references (identifier and timestamp). Fetch the full tracks
    /// via the track endpoints using the referenced identifiers.
    /// </summary>
    public IReadOnlyList<TrackShort> Tracks { get; init; } = [];
}

/// <summary>A liked album together with when it was liked.</summary>
public sealed class LikedAlbum
{
    /// <summary>The album identifier.</summary>
    public long Id { get; init; }

    /// <summary>When the album was liked, when known.</summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>The album.</summary>
    public Album? Album { get; init; }
}

/// <summary>The envelope the likes/tracks endpoint wraps the liked tracks in (<c>{ "library": { … } }</c>).</summary>
internal sealed class LikedTracksEnvelope
{
    /// <summary>The liked-tracks payload.</summary>
    public LikedTracks Library { get; init; } = new();
}
