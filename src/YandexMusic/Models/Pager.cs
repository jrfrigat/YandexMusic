namespace YandexMusic.Models;

/// <summary>Pagination information for a list endpoint.</summary>
public sealed class Pager
{
    /// <summary>The total number of items across all pages.</summary>
    public int Total { get; init; }

    /// <summary>The zero-based current page index.</summary>
    public int Page { get; init; }

    /// <summary>The number of items per page.</summary>
    public int PerPage { get; init; }
}
