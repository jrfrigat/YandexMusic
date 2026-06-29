namespace YandexMusic.Models.Metatags;

/// <summary>The full metatag tree: one branch per navigation section (moods, activities, genres, epochs).</summary>
public sealed class Metatags
{
    /// <summary>The navigation branches that make up the tree.</summary>
    public IReadOnlyList<MetatagTree> Trees { get; init; } = [];
}

/// <summary>A single navigation branch of the metatag tree.</summary>
public sealed class MetatagTree
{
    /// <summary>The localized branch title.</summary>
    public string? Title { get; init; }

    /// <summary>The navigation section identifier (moods, activities, genres or epochs).</summary>
    public string? NavigationId { get; init; }

    /// <summary>The leaves (individual metatags) under this branch.</summary>
    public IReadOnlyList<MetatagLeaf> Leaves { get; init; } = [];
}

/// <summary>A leaf of the metatag tree, optionally containing nested leaves.</summary>
public sealed class MetatagLeaf
{
    /// <summary>The metatag identifier, used as the <c>metatagId</c> in the endpoints.</summary>
    public string? Tag { get; init; }

    /// <summary>The localized leaf title.</summary>
    public string? Title { get; init; }

    /// <summary>The nested child leaves.</summary>
    public IReadOnlyList<MetatagLeaf> Leaves { get; init; } = [];
}
