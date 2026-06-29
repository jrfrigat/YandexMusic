using YandexMusic.Models;
using YandexMusic.Models.Account;
using YandexMusic.Models.Playlists;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving and managing playlists.</summary>
public interface IPlaylistsClient
{
    /// <summary>Gets a playlist by its owner and kind.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind, unique per owner.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The playlist, or <see langword="null"/> if it does not exist.</returns>
    Task<Playlist?> GetAsync(string userId, string kind, CancellationToken cancellationToken = default);

    /// <summary>Gets all playlists owned by a user.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's playlists.</returns>
    Task<IReadOnlyList<Playlist>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Gets a user's playback and privacy settings.</summary>
    /// <param name="userId">The user identifier whose settings to read.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's settings, or <see langword="null"/> when not available.</returns>
    Task<UserSettings?> GetUserSettingsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Gets several of a user's playlists by their kinds in a single request.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kinds">The playlist kinds to fetch.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested playlists.</returns>
    Task<IReadOnlyList<Playlist>> GetManyAsync(string userId, IReadOnlyList<string> kinds, CancellationToken cancellationToken = default);

    /// <summary>Gets the personalized track recommendations for one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind, unique per owner.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The recommendations, or <see langword="null"/> when none are available.</returns>
    Task<PlaylistRecommendations?> GetRecommendationsAsync(string userId, string kind, CancellationToken cancellationToken = default);

    /// <summary>Creates a new playlist for a user.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="title">The playlist title.</param>
    /// <param name="visibility">The playlist visibility. Defaults to <see cref="PlaylistVisibility.Public"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created playlist, or <see langword="null"/> when creation failed.</returns>
    Task<Playlist?> CreateAsync(string userId, string title, PlaylistVisibility visibility = PlaylistVisibility.Public, CancellationToken cancellationToken = default);

    /// <summary>Deletes one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to delete.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the playlist was deleted.</returns>
    Task<bool> DeleteAsync(string userId, string kind, CancellationToken cancellationToken = default);

    /// <summary>Renames one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to rename.</param>
    /// <param name="value">The new playlist name.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated playlist, or <see langword="null"/> on failure.</returns>
    Task<Playlist?> RenameAsync(string userId, string kind, string value, CancellationToken cancellationToken = default);

    /// <summary>Changes the visibility of one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to update.</param>
    /// <param name="visibility">The new visibility.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated playlist, or <see langword="null"/> on failure.</returns>
    Task<Playlist?> SetVisibilityAsync(string userId, string kind, PlaylistVisibility visibility, CancellationToken cancellationToken = default);

    /// <summary>Sets the description of one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to update.</param>
    /// <param name="value">The new description.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated playlist, or <see langword="null"/> on failure.</returns>
    Task<Playlist?> SetDescriptionAsync(string userId, string kind, string value, CancellationToken cancellationToken = default);

    /// <summary>Applies a raw difference to one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to change.</param>
    /// <param name="diff">The JSON difference describing the change to apply.</param>
    /// <param name="revision">The current playlist revision for optimistic concurrency. Defaults to <c>1</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated playlist, or <see langword="null"/> on failure.</returns>
    Task<Playlist?> ChangeAsync(string userId, string kind, string diff, int revision = 1, CancellationToken cancellationToken = default);

    /// <summary>Inserts a track into one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to change.</param>
    /// <param name="trackId">The identifier of the track to insert.</param>
    /// <param name="albumId">The identifier of the album the track belongs to.</param>
    /// <param name="at">The zero-based index to insert at. Defaults to <c>0</c>.</param>
    /// <param name="revision">The current playlist revision. Defaults to <c>1</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated playlist, or <see langword="null"/> on failure.</returns>
    Task<Playlist?> InsertTrackAsync(string userId, string kind, string trackId, string albumId, int at = 0, int revision = 1, CancellationToken cancellationToken = default);

    /// <summary>Removes a range of tracks from one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind to change.</param>
    /// <param name="from">The zero-based start index of the range to remove.</param>
    /// <param name="to">The exclusive end index of the range to remove.</param>
    /// <param name="revision">The current playlist revision. Defaults to <c>1</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated playlist, or <see langword="null"/> on failure.</returns>
    Task<Playlist?> DeleteTracksAsync(string userId, string kind, int from, int to, int revision = 1, CancellationToken cancellationToken = default);

    /// <summary>Joins a collaborative playlist using an invitation token.</summary>
    /// <param name="userId">The identifier of the playlist owner.</param>
    /// <param name="token">The invitation token.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the playlist was joined.</returns>
    Task<bool> JoinCollectiveAsync(string userId, string token, CancellationToken cancellationToken = default);

    /// <summary>Gets a playlist by its globally-unique identifier.</summary>
    /// <param name="playlistUuid">The playlist's globally-unique identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The playlist, or <see langword="null"/> when not found.</returns>
    Task<Playlist?> GetByUuidAsync(string playlistUuid, CancellationToken cancellationToken = default);

    /// <summary>Gets the entities similar to a playlist.</summary>
    /// <param name="playlistUuid">The playlist's globally-unique identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The similar entities, or <see langword="null"/> when none are available.</returns>
    Task<PlaylistSimilarEntities?> GetSimilarEntitiesAsync(string playlistUuid, CancellationToken cancellationToken = default);

    /// <summary>Gets several playlists by their <c>uid:kind</c> identifiers.</summary>
    /// <param name="playlistIds">The playlist identifiers, each in <c>uid:kind</c> format.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching playlists, or <see langword="null"/> when none are returned.</returns>
    Task<PlaylistsList?> GetByIdsAsync(IReadOnlyList<string> playlistIds, CancellationToken cancellationToken = default);

    /// <summary>Gets several playlists by their identifiers, without their tracks populated.</summary>
    /// <param name="playlistIds">The playlist identifiers, each in <c>owner_id:playlist_id</c> format.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The shortened playlists.</returns>
    Task<IReadOnlyList<Playlist>> GetShortListAsync(IReadOnlyList<string> playlistIds, CancellationToken cancellationToken = default);

    /// <summary>Gets a personal auto-generated playlist by its identifier.</summary>
    /// <param name="playlistId">The generator identifier, for example <c>daily</c>, <c>missedLikes</c>, <c>recentTracks</c>, <c>neverHeard</c>, <c>podcasts</c> or <c>origin</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The generated playlist, or <see langword="null"/> when not available.</returns>
    Task<GeneratedPlaylist?> GetPersonalAsync(string playlistId, CancellationToken cancellationToken = default);

    /// <summary>Gets the trailer configured for one of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="kind">The playlist kind, unique per owner.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The trailer, or <see langword="null"/> when none is configured.</returns>
    Task<PlaylistTrailer?> GetTrailerAsync(string userId, string kind, CancellationToken cancellationToken = default);

    /// <summary>Gets the kinds of all of a user's playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The playlist kinds.</returns>
    Task<IReadOnlyList<string>> GetKindsAsync(string userId, CancellationToken cancellationToken = default);
}
