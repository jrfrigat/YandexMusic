using YandexMusic.Models;
using YandexMusic.Models.Clips;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for retrieving music clips.</summary>
public interface IClipsClient
{
    /// <summary>Gets clips by their identifiers.</summary>
    /// <param name="clipIds">The clip identifiers.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The requested clips. Empty when none are supplied.</returns>
    Task<IReadOnlyList<Clip>> GetManyAsync(IEnumerable<string> clipIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a personalized page of recommended clips. Requires authentication.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The number of clips per page.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The recommended clips, or <see langword="null"/> when none are available.</returns>
    Task<ClipsWillLike?> GetWillLikeAsync(int page = 0, int pageSize = 50, CancellationToken cancellationToken = default);
}
