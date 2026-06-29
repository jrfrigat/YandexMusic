namespace YandexMusic.Models;

/// <summary>A structural block of a skeleton (the placeholder layout of a landing-style page).</summary>
public sealed class SkeletonBlock
{
    /// <summary>The block identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The block type. Known values include <c>TABS</c>, <c>ARTIST_CONCERTS</c>, <c>CONCERT_PLACE</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The block payload, when present.</summary>
    public SkeletonBlockData? Data { get; init; }
}

/// <summary>The payload of a <see cref="SkeletonBlock"/>.</summary>
public sealed class SkeletonBlockData
{
    /// <summary>The tabs in the block, when it is a tab container.</summary>
    public IReadOnlyList<SkeletonTab>? Tabs { get; init; }

    /// <summary>The index of the initially selected tab.</summary>
    public int? SelectedTabIndex { get; init; }

    /// <summary>The data source of the block, when present.</summary>
    public SkeletonSource? Source { get; init; }

    /// <summary>The block title, when present.</summary>
    public string? Title { get; init; }

    /// <summary>The display policy of the block, when present.</summary>
    public string? ShowPolicy { get; init; }

    /// <summary>The "view all" action of the block, when present.</summary>
    public SkeletonViewAllAction? ViewAllAction { get; init; }
}

/// <summary>A tab within a <see cref="SkeletonBlockData"/>.</summary>
public sealed class SkeletonTab
{
    /// <summary>The tab identifier.</summary>
    public string? Id { get; init; }

    /// <summary>The tab title.</summary>
    public string? Title { get; init; }

    /// <summary>The blocks shown under the tab.</summary>
    public IReadOnlyList<SkeletonBlock>? Blocks { get; init; }
}

/// <summary>The data source backing a skeleton block.</summary>
public sealed class SkeletonSource
{
    /// <summary>The source URI.</summary>
    public string? Uri { get; init; }

    /// <summary>The item count reported for the web client.</summary>
    public int? CountWeb { get; init; }

    /// <summary>The item count.</summary>
    public int? Count { get; init; }
}

/// <summary>The action triggered by a skeleton block's "view all" control.</summary>
public sealed class SkeletonViewAllAction
{
    /// <summary>The in-app deep link.</summary>
    public string? Deeplink { get; init; }

    /// <summary>The web link.</summary>
    public string? Weblink { get; init; }
}
