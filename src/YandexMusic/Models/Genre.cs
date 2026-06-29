namespace YandexMusic.Models;

/// <summary>A localized title pair for a <see cref="Genre"/>.</summary>
public sealed class GenreTitle
{
    /// <summary>The short title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The full title, when different from the short one.</summary>
    public string? FullTitle { get; init; }
}

/// <summary>A music genre, which may contain sub-genres.</summary>
public sealed class Genre
{
    /// <summary>The genre identifier (for example <c>"rock"</c>).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The sort weight used to order genres.</summary>
    public int Weight { get; init; }

    /// <summary>Whether the genre is shown in the composer top list.</summary>
    public bool ComposerTop { get; init; }

    /// <summary>The genre title in the default language.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The full genre title in the default language, when different.</summary>
    public string? FullTitle { get; init; }

    /// <summary>The genre titles keyed by language code (for example <c>"ru"</c>, <c>"en"</c>).</summary>
    public IReadOnlyDictionary<string, GenreTitle>? Titles { get; init; }

    /// <summary>The URL part the genre is exposed under, when present.</summary>
    public string? Url { get; init; }

    /// <summary>The accent colour associated with the genre, when present.</summary>
    public string? Color { get; init; }

    /// <summary>The sub-genres, when present.</summary>
    public IReadOnlyList<Genre>? SubGenres { get; init; }
}
