using YandexMusic.Models;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the catalogue's genre tree.</summary>
public interface IGenresClient
{
    /// <summary>Gets all genres (with their sub-genres).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The list of genres.</returns>
    Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
}
