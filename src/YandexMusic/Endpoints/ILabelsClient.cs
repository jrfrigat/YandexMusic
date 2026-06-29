using YandexMusic.Models;
using YandexMusic.Models.Labels;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving record labels and their catalogue.</summary>
public interface ILabelsClient
{
    /// <summary>Gets a single label by its identifier.</summary>
    /// <param name="labelId">The label identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The label, or <see langword="null"/> if it does not exist.</returns>
    Task<Label?> GetAsync(string labelId, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of the albums released by a label.</summary>
    /// <param name="labelId">The label identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of albums per page.</param>
    /// <param name="sortBy">The sort field. Known values: <c>year</c>, <c>rating</c>. Omitted when <see langword="null"/>.</param>
    /// <param name="sortOrder">The sort direction. Known values: <c>asc</c>, <c>desc</c>. Omitted when <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page of albums, or <see langword="null"/> if the label does not exist.</returns>
    Task<LabelAlbums?> GetAlbumsAsync(
        string labelId,
        int page = 0,
        int pageSize = 100,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a page of the artists signed to a label.</summary>
    /// <param name="labelId">The label identifier.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of artists per page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The page of artists, or <see langword="null"/> if the label does not exist.</returns>
    Task<LabelArtists?> GetArtistsAsync(
        string labelId,
        int page = 0,
        int pageSize = 100,
        CancellationToken cancellationToken = default);
}
