namespace YandexMusic.Player.Catalog;

/// <summary>
/// The app's view of the music service. It hides the underlying <c>IYandexMusicClient</c> behind a few
/// task-friendly methods and the UI's own view-models, so screens and playback never depend on the
/// library's models directly.
/// </summary>
public interface IMusicCatalog
{
    /// <summary>Searches the catalogue for tracks.</summary>
    /// <param name="query">The search text.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching tracks.</returns>
    Task<IReadOnlyList<TrackView>> SearchTracksAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>Gets the signed-in user's liked albums.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's albums.</returns>
    Task<IReadOnlyList<AlbumView>> GetMyAlbumsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets an album together with its tracklist.</summary>
    /// <param name="albumId">The album identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The album and its tracks, or <see langword="null"/> when not found.</returns>
    Task<AlbumDetail?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Gets the signed-in user's own playlists.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's playlists.</returns>
    Task<IReadOnlyList<PlaylistView>> GetMyPlaylistsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets one of the user's playlists together with its tracklist.</summary>
    /// <param name="playlistId">The playlist kind, as returned in <see cref="PlaylistView.Id"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The playlist and its tracks, or <see langword="null"/> when not found.</returns>
    Task<PlaylistDetail?> GetPlaylistAsync(string playlistId, CancellationToken cancellationToken = default);

    /// <summary>Gets the user's "liked" tracks (most recent first).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The liked tracks.</returns>
    Task<IReadOnlyList<TrackView>> GetLikedTracksAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a batch of tracks from the user's personal "My Wave" radio station.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The wave's tracks.</returns>
    Task<IReadOnlyList<TrackView>> GetMyWaveAsync(CancellationToken cancellationToken = default);

    /// <summary>Resolves a direct media URL for a track, when one is available.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The media URL, or <see langword="null"/> when unavailable (no subscription/token).</returns>
    Task<string?> ResolveStreamUrlAsync(string trackId, CancellationToken cancellationToken = default);
}
