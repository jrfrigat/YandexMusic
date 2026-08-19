namespace YandexMusic.Player.Catalog;

/// <summary>
/// The app's view of the music service. It hides the underlying <c>IYandexMusicClient</c> behind a few
/// task-friendly methods and the UI's own view-models, so screens and playback never depend on the
/// library's models directly.
/// </summary>
public interface IMusicCatalog
{
    /// <summary>Searches the catalogue for tracks, one page at a time.</summary>
    /// <param name="query">The search text.</param>
    /// <param name="page">The zero-based result page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page of matching tracks and the total match count.</returns>
    Task<SearchPage<TrackView>> SearchTracksAsync(string query, int page = 0, CancellationToken cancellationToken = default);

    /// <summary>Searches the catalogue for albums, one page at a time.</summary>
    /// <param name="query">The search text.</param>
    /// <param name="page">The zero-based result page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page of matching albums and the total match count.</returns>
    Task<SearchPage<AlbumView>> SearchAlbumsAsync(string query, int page = 0, CancellationToken cancellationToken = default);

    /// <summary>Searches the catalogue for playlists, one page at a time.</summary>
    /// <param name="query">The search text.</param>
    /// <param name="page">The zero-based result page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page of matching playlists and the total match count.</returns>
    Task<SearchPage<PlaylistView>> SearchPlaylistsAsync(string query, int page = 0, CancellationToken cancellationToken = default);

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

    /// <summary>Gets the identifiers of the user's liked tracks, for like-state indicators.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The liked track ids.</returns>
    Task<IReadOnlyList<string>> GetLikedTrackIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds or removes the "liked" mark of a track.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="liked"><see langword="true"/> to like, <see langword="false"/> to un-like.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>Whether the API accepted the change.</returns>
    Task<bool> SetTrackLikedAsync(string trackId, bool liked, CancellationToken cancellationToken = default);

    /// <summary>Marks a track as disliked (and removes the like), steering future recommendations away.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>Whether the API accepted the change.</returns>
    Task<bool> DislikeTrackAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Gets a batch of tracks from the user's personal "My Wave" radio station.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The wave batch.</returns>
    Task<RadioBatch> GetMyWaveAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a batch from an arbitrary radio station, identified as <c>type:tag</c>.</summary>
    /// <param name="station">The station identity, for example <c>user:onyourwave</c> or <c>track:{id}</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The station batch.</returns>
    Task<RadioBatch> GetRadioAsync(string station, CancellationToken cancellationToken = default);

    /// <summary>Gets a batch of a radio station seeded from a track (its "similar tracks" stream).</summary>
    /// <param name="trackId">The track to seed the station with.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The similar-tracks batch.</returns>
    Task<RadioBatch> GetSimilarRadioAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Downloads the lyrics of a track, when they are available.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The lyrics text, or <see langword="null"/> when the track has none.</returns>
    Task<string?> GetLyricsAsync(string trackId, CancellationToken cancellationToken = default);

    /// <summary>Resolves a direct media URL for a track, when one is available.</summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The media URL, or <see langword="null"/> when unavailable (no subscription/token).</returns>
    Task<string?> ResolveStreamUrlAsync(string trackId, CancellationToken cancellationToken = default);
}
