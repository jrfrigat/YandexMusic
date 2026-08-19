namespace YandexMusic.Player.Playback;

/// <summary>The state of an <see cref="IAudioPlayer"/>.</summary>
public enum PlaybackState
{
    /// <summary>Nothing is loaded.</summary>
    Idle,

    /// <summary>A track is being loaded/buffered.</summary>
    Buffering,

    /// <summary>Audio is playing.</summary>
    Playing,

    /// <summary>Playback is paused.</summary>
    Paused,

    /// <summary>Playback was stopped by the user.</summary>
    Stopped,

    /// <summary>The current track finished on its own.</summary>
    Ended,

    /// <summary>Loading or playback failed.</summary>
    Error,
}

/// <summary>
/// Where a playback queue came from. It travels with every <see cref="PlaybackItem"/> so the
/// play-reporting and radio feedback can tell the API what the user is listening to, and radio
/// queues know which station to fetch the next batch from.
/// </summary>
/// <param name="From">The analytics "from" label (for example "search", "album", "mywave").</param>
/// <param name="AlbumId">The album the queue was built from, when applicable.</param>
/// <param name="PlaylistId">The playlist the queue was built from, when applicable.</param>
/// <param name="Station">The radio station identity, when the queue is a radio batch.</param>
/// <param name="BatchId">The radio batch the item arrived in; refreshed on every continuation.</param>
public sealed record PlaybackOrigin(
    string From,
    string? AlbumId = null,
    string? PlaylistId = null,
    string? Station = null,
    string? BatchId = null);

/// <summary>
/// A single thing to play. The audio URL is resolved lazily so the queue can hold tracks long before
/// (and without ever) fetching a stream — useful when there is no subscription/token.
/// </summary>
/// <param name="Id">The track identifier.</param>
/// <param name="Title">The track title.</param>
/// <param name="Artist">The display artist(s).</param>
/// <param name="Duration">The known track duration.</param>
/// <param name="ResolveStreamUrlAsync">Resolves a direct media URL, or <see langword="null"/> when unavailable.</param>
/// <param name="Origin">Where the track's queue came from, for play reporting.</param>
public sealed record PlaybackItem(
    string Id,
    string Title,
    string Artist,
    TimeSpan Duration,
    Func<CancellationToken, Task<string?>> ResolveStreamUrlAsync,
    PlaybackOrigin? Origin = null);
