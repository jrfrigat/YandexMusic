using YandexMusic.Models.Pins;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the items a user has pinned to the top of their library.</summary>
public interface IPinsClient
{
    /// <summary>Gets the authenticated user's pinned items.</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The pinned items, or <see langword="null"/> if unavailable.</returns>
    Task<PinsList?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Pins an album to the top of the user's library.</summary>
    /// <param name="albumId">The album identifier to pin.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created pin, or <see langword="null"/> if the server returned no entry.</returns>
    Task<Pin?> PinAlbumAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Removes an album pin from the user's library.</summary>
    /// <param name="albumId">The album identifier to unpin.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the server confirmed the removal.</returns>
    Task<bool> UnpinAlbumAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Pins an artist to the top of the user's library.</summary>
    /// <param name="artistId">The artist identifier to pin.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created pin, or <see langword="null"/> if the server returned no entry.</returns>
    Task<Pin?> PinArtistAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Removes an artist pin from the user's library.</summary>
    /// <param name="artistId">The artist identifier to unpin.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the server confirmed the removal.</returns>
    Task<bool> UnpinArtistAsync(string artistId, CancellationToken cancellationToken = default);

    /// <summary>Pins a playlist to the top of the user's library.</summary>
    /// <param name="userId">The playlist owner identifier.</param>
    /// <param name="kind">The playlist number within its owner's library.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created pin, or <see langword="null"/> if the server returned no entry.</returns>
    Task<Pin?> PinPlaylistAsync(string userId, string kind, CancellationToken cancellationToken = default);

    /// <summary>Removes a playlist pin from the user's library.</summary>
    /// <param name="userId">The playlist owner identifier.</param>
    /// <param name="kind">The playlist number within its owner's library.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the server confirmed the removal.</returns>
    Task<bool> UnpinPlaylistAsync(string userId, string kind, CancellationToken cancellationToken = default);

    /// <summary>Pins a personalised wave to the top of the user's library.</summary>
    /// <param name="seeds">The wave seed identifier, for example <c>artist:12345</c>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created pin, or <see langword="null"/> if the server returned no entry.</returns>
    Task<Pin?> PinWaveAsync(string seeds, CancellationToken cancellationToken = default);

    /// <summary>Removes a wave pin from the user's library.</summary>
    /// <param name="seeds">The wave seed identifier to unpin.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the server confirmed the removal.</returns>
    Task<bool> UnpinWaveAsync(string seeds, CancellationToken cancellationToken = default);
}
