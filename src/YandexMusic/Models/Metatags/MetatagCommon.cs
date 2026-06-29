namespace YandexMusic.Models.Metatags;

/// <summary>The title pair (short and full) shown for a metatag landing page.</summary>
public sealed class MetatagTitle
{
    /// <summary>The short title.</summary>
    public string? Title { get; init; }

    /// <summary>The full title.</summary>
    public string? FullTitle { get; init; }
}

/// <summary>One selectable sort/period option offered by a metatag landing page.</summary>
public sealed class MetatagSortByValue
{
    /// <summary>The sort value (for example <c>popular</c> or <c>new</c>).</summary>
    public string? Value { get; init; }

    /// <summary>The localized option title.</summary>
    public string? Title { get; init; }

    /// <summary>Whether this option is the currently selected one.</summary>
    public bool? Active { get; init; }
}
