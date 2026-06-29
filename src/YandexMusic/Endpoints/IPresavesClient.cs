using YandexMusic.Models.Presaves;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for a user's pre-saved (pre-released) albums.</summary>
public interface IPresavesClient
{
    /// <summary>Gets a user's pre-saved albums.</summary>
    /// <param name="userId">The owner's user identifier.</param>
    /// <param name="includeReleased">Whether to include albums that have already been released.</param>
    /// <param name="includeUpcoming">Whether to include albums that are not yet released.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user's pre-saved albums, or <see langword="null"/> if unavailable.</returns>
    Task<Presaves?> GetAsync(
        string userId,
        bool includeReleased = false,
        bool includeUpcoming = true,
        CancellationToken cancellationToken = default);

    /// <summary>Pre-saves an album for a user.</summary>
    /// <param name="userId">The owner's user identifier.</param>
    /// <param name="albumId">The identifier of the album to pre-save.</param>
    /// <param name="likeAfterRelease">Whether to automatically like the album once it is released.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> if the album was pre-saved successfully.</returns>
    Task<bool> AddAsync(
        string userId,
        string albumId,
        bool likeAfterRelease = true,
        CancellationToken cancellationToken = default);

    /// <summary>Removes an album from a user's pre-saves.</summary>
    /// <param name="userId">The owner's user identifier.</param>
    /// <param name="albumId">The identifier of the album to remove.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> if the album was removed successfully.</returns>
    Task<bool> RemoveAsync(string userId, string albumId, CancellationToken cancellationToken = default);
}
