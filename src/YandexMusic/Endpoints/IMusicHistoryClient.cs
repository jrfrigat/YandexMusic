using YandexMusic.Models.MusicHistory;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for the signed-in user's listening history.</summary>
public interface IMusicHistoryClient
{
    /// <summary>Gets the user's listening history, grouped into day tabs.</summary>
    /// <param name="fullModelsCount">
    /// The number of entries for which the API should embed the resolved full model; defaults to <c>0</c>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The listening history, or <see langword="null"/> when unavailable.</returns>
    Task<MusicHistory?> GetAsync(int fullModelsCount = 0, CancellationToken cancellationToken = default);

    /// <summary>Resolves a set of history item identifiers into their full models.</summary>
    /// <param name="items">The identifiers to resolve, each tagged with its type.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The resolved items, or <see langword="null"/> when unavailable.</returns>
    Task<MusicHistoryItems?> GetItemsAsync(IReadOnlyList<MusicHistoryItemRequest> items, CancellationToken cancellationToken = default);
}
