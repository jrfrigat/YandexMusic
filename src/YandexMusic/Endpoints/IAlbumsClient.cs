using YandexMusic.Models.Albums;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving albums from the catalogue.</summary>
public interface IAlbumsClient
{
    /// <summary>Gets a single album by its identifier.</summary>
    /// <param name="albumId">The album identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The album, or <see langword="null"/> if it does not exist.</returns>
    Task<Album?> GetAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Gets several albums by their identifiers in a single request.</summary>
    /// <param name="albumIds">The album identifiers.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The albums that were found. Empty when none are supplied.</returns>
    Task<IReadOnlyList<Album>> GetManyAsync(IEnumerable<string> albumIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an album together with its tracks, grouped by disc in <see cref="Album.Volumes"/>.
    /// </summary>
    /// <param name="albumId">The album identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The album with its tracks, or <see langword="null"/> if it does not exist.</returns>
    Task<Album?> GetWithTracksAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Gets entities similar to an album, such as personalised wave recommendations.</summary>
    /// <param name="albumId">The album identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The similar entities, or <see langword="null"/> if none are available.</returns>
    Task<AlbumSimilarEntities?> GetSimilarEntitiesAsync(string albumId, CancellationToken cancellationToken = default);

    /// <summary>Gets the trailer of an album, including its artists and trailer tracks.</summary>
    /// <param name="albumId">The album identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The album trailer, or <see langword="null"/> if the album has none.</returns>
    Task<AlbumTrailer?> GetTrailerAsync(string albumId, CancellationToken cancellationToken = default);
}
