namespace YandexMusic.Models.Search;

/// <summary>Autocomplete suggestions for a partial search query.</summary>
public sealed class SearchSuggestions
{
    /// <summary>The single best match for the partial query, when available.</summary>
    public SearchBest? Best { get; init; }

    /// <summary>The suggested completion strings.</summary>
    public IReadOnlyList<string> Suggestions { get; init; } = [];
}
