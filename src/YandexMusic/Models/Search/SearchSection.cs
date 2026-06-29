namespace YandexMusic.Models.Search;

/// <summary>
/// One category of search results (for example tracks or albums), with paging information.
/// </summary>
/// <typeparam name="T">The type of item in this section.</typeparam>
public sealed class SearchSection<T>
{
    /// <summary>The total number of matches across all pages.</summary>
    public int Total { get; init; }

    /// <summary>The number of items returned per page.</summary>
    public int PerPage { get; init; }

    /// <summary>The order the results are sorted in, as reported by the API.</summary>
    public int Order { get; init; }

    /// <summary>The matches on the current page.</summary>
    public IReadOnlyList<T> Results { get; init; } = [];
}
