using YandexMusic.Models.Search;

namespace YandexMusic.Endpoints;

/// <summary>Endpoints for searching the catalogue.</summary>
public interface ISearchClient
{
    /// <summary>Searches the catalogue.</summary>
    /// <param name="text">The query text.</param>
    /// <param name="type">The category to restrict the search to. Defaults to <see cref="SearchType.All"/>.</param>
    /// <param name="page">The zero-based result page.</param>
    /// <param name="correctMisspells">
    /// Whether the API may automatically correct a misspelled query. Defaults to <see langword="true"/>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The search result, or <see langword="null"/> if the API returned nothing.</returns>
    Task<SearchResult?> SearchAsync(
        string text,
        SearchType type = SearchType.All,
        int page = 0,
        bool correctMisspells = true,
        CancellationToken cancellationToken = default);

    /// <summary>Gets autocomplete suggestions for a partial query.</summary>
    /// <param name="part">The partial query text.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The suggestions, or <see langword="null"/> if the API returned nothing.</returns>
    Task<SearchSuggestions?> SuggestAsync(string part, CancellationToken cancellationToken = default);
}
