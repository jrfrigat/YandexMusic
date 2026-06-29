using YandexMusic.Models.Artists;
using YandexMusic.Models.Clips;
using YandexMusic.Models.Library;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for a user's library (likes and dislikes).</summary>
public interface ILibraryClient
{
    /// <summary>Gets a user's liked tracks (as lightweight references).</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="ifModifiedSinceRevision">Return data only if newer than this library revision; <c>0</c> always returns it.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's liked tracks, or <see langword="null"/> if unavailable.</returns>
    Task<LikedTracks?> GetLikedTracksAsync(string userId, int ifModifiedSinceRevision = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets a user's liked albums.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="rich">Whether to return the full album payload rather than a shortened one.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's liked albums.</returns>
    Task<IReadOnlyList<LikedAlbum>> GetLikedAlbumsAsync(string userId, bool rich = true, CancellationToken cancellationToken = default);

    /// <summary>Gets a user's liked artists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="withTimestamps">Whether to include the like timestamps.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's liked artists.</returns>
    Task<IReadOnlyList<Artist>> GetLikedArtistsAsync(string userId, bool withTimestamps = true, CancellationToken cancellationToken = default);

    /// <summary>Gets a user's liked playlists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's liked playlists.</returns>
    Task<IReadOnlyList<Like>> GetLikedPlaylistsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Adds tracks to a user's likes (clearing any matching dislike).</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="trackIds">The track identifiers to like.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was applied.</returns>
    Task<bool> AddLikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default);

    /// <summary>Removes tracks from a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="trackIds">The track identifiers to unlike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was removed.</returns>
    Task<bool> RemoveLikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default);

    /// <summary>Adds albums to a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="albumIds">The album identifiers to like.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was applied.</returns>
    Task<bool> AddLikedAlbumsAsync(string userId, IEnumerable<string> albumIds, CancellationToken cancellationToken = default);

    /// <summary>Removes albums from a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="albumIds">The album identifiers to unlike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was removed.</returns>
    Task<bool> RemoveLikedAlbumsAsync(string userId, IEnumerable<string> albumIds, CancellationToken cancellationToken = default);

    /// <summary>Adds artists to a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="artistIds">The artist identifiers to like.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was applied.</returns>
    Task<bool> AddLikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default);

    /// <summary>Removes artists from a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="artistIds">The artist identifiers to unlike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was removed.</returns>
    Task<bool> RemoveLikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default);

    /// <summary>Adds playlists to a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="playlistIds">The playlist identifiers (each in <c>owner-id:playlist-id</c> form) to like.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was applied.</returns>
    Task<bool> AddLikedPlaylistsAsync(string userId, IEnumerable<string> playlistIds, CancellationToken cancellationToken = default);

    /// <summary>Removes playlists from a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="playlistIds">The playlist identifiers (each in <c>owner-id:playlist-id</c> form) to unlike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was removed.</returns>
    Task<bool> RemoveLikedPlaylistsAsync(string userId, IEnumerable<string> playlistIds, CancellationToken cancellationToken = default);

    /// <summary>Gets a user's disliked tracks (as lightweight references).</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="ifModifiedSinceRevision">Return data only if newer than this library revision; <c>0</c> always returns it.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's disliked tracks, or <see langword="null"/> if unavailable.</returns>
    Task<LikedTracks?> GetDislikedTracksAsync(string userId, int ifModifiedSinceRevision = 0, CancellationToken cancellationToken = default);

    /// <summary>Gets a user's disliked artists.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's disliked artists.</returns>
    Task<IReadOnlyList<Artist>> GetDislikedArtistsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Adds tracks to a user's dislikes (clearing any matching like).</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="trackIds">The track identifiers to dislike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the dislike was applied.</returns>
    Task<bool> AddDislikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default);

    /// <summary>Removes tracks from a user's dislikes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="trackIds">The track identifiers to undislike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the dislike was removed.</returns>
    Task<bool> RemoveDislikedTracksAsync(string userId, IEnumerable<string> trackIds, CancellationToken cancellationToken = default);

    /// <summary>Adds artists to a user's dislikes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="artistIds">The artist identifiers to dislike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the dislike was applied.</returns>
    Task<bool> AddDislikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default);

    /// <summary>Removes artists from a user's dislikes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="artistIds">The artist identifiers to undislike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the dislike was removed.</returns>
    Task<bool> RemoveDislikedArtistsAsync(string userId, IEnumerable<string> artistIds, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of a user's liked clips.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of clips per page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page of liked clips, or <see langword="null"/> if unavailable.</returns>
    Task<ClipsWillLike?> GetLikedClipsAsync(string userId, int page = 0, int pageSize = 100, CancellationToken cancellationToken = default);

    /// <summary>Adds a clip to a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="clipId">The clip identifier to like.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was applied.</returns>
    Task<bool> AddLikedClipAsync(string userId, string clipId, CancellationToken cancellationToken = default);

    /// <summary>Removes a clip from a user's likes.</summary>
    /// <param name="userId">The owner's login or user identifier.</param>
    /// <param name="clipId">The clip identifier to unlike.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the like was removed.</returns>
    Task<bool> RemoveLikedClipAsync(string userId, string clipId, CancellationToken cancellationToken = default);
}
