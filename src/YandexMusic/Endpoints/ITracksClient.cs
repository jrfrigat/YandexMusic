using YandexMusic.Models.Tracks;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving tracks from the catalogue.</summary>
public interface ITracksClient
{
    /// <summary>Gets a single track by its identifier.</summary>
    /// <param name="trackId">The track identifier (for example <c>"4"</c>).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The track, or <see langword="null"/> if it does not exist.</returns>
    Task<Track?> GetAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets several tracks by their identifiers in a single request.</summary>
    /// <param name="trackIds">The track identifiers.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The tracks that were found, in the order the API returns them.</returns>
    Task<IReadOnlyList<Track>> GetManyAsync(IEnumerable<string> trackIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available download/stream variants for a track. Requires an authenticated session
    /// with the appropriate subscription.
    /// </summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The available variants (codecs and bitrates).</returns>
    Task<IReadOnlyList<DownloadInfo>> GetDownloadInfoAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a direct, signed MP3 URL for a track by picking the highest-bitrate non-preview MP3
    /// variant and following its download-info granule. Requires an authenticated session with the
    /// appropriate subscription.
    /// </summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The direct media URL, or <see langword="null"/> if no downloadable variant exists.</returns>
    Task<string?> GetDirectLinkAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets supplementary information for a track, such as its lyrics.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The supplement, or <see langword="null"/> if none is available.</returns>
    Task<TrackSupplement?> GetSupplementAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets tracks similar to the given track.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The track and its similar tracks, or <see langword="null"/> if none are available.</returns>
    Task<SimilarTracks?> GetSimilarAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the lyrics metadata for a track. Requires authentication. The returned
    /// <see cref="TrackLyrics.DownloadUrl"/> hosts the actual text.
    /// </summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="format">The lyrics format. Known values: <c>TEXT</c> (plain) and <c>LRC</c> (timed).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The lyrics metadata, or <see langword="null"/> if the track has no lyrics.</returns>
    Task<TrackLyrics?> GetLyricsAsync(string trackId, string format = "TEXT", CancellationToken cancellationToken = default);

    /// <summary>Gets extended information about a track, including related tracks and aliases.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The full information, or <see langword="null"/> if it is unavailable.</returns>
    Task<TrackFullInfo?> GetFullInfoAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets the trailer of a track (used by podcasts and audiobooks).</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The trailer, or <see langword="null"/> if the track has none.</returns>
    Task<TrackTrailer?> GetTrailerAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports playback progress for a track. Used to keep the "recently played" history and playback
    /// position in sync. Requires authentication.
    /// </summary>
    /// <param name="options">The playback report.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the server accepted the report.</returns>
    Task<bool> PlayAudioAsync(PlayAudioOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Alice shot or advertisement to play after a track. When <paramref name="context"/> is
    /// <c>playlist</c>, <paramref name="contextItem"/> must be <c>{ownerId}:{playlistId}</c>.
    /// </summary>
    /// <param name="nextTrackId">The identifier of the track about to play.</param>
    /// <param name="contextItem">The context identifier (for a playlist, <c>{ownerId}:{playlistId}</c>).</param>
    /// <param name="prevTrackId">The identifier of the track that just finished, when known.</param>
    /// <param name="context">The context the playback originates from. Known value: <c>playlist</c>.</param>
    /// <param name="types">What to return after the track. Known values: <c>shot</c>, <c>ad</c>.</param>
    /// <param name="from">Where the user entered the context from.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The shot event, or <see langword="null"/> if none is available.</returns>
    Task<ShotEvent?> GetAfterTrackAsync(
        string nextTrackId,
        string contextItem,
        string? prevTrackId = null,
        string context = "playlist",
        string types = "shot",
        string from = "mobile-landing-origin-default",
        CancellationToken cancellationToken = default);
}
