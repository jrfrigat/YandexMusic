using YandexMusic.Models.Playlists;

namespace YandexMusic.Models.Landing;

/// <summary>The chart landing payload: the chart playlist plus the territory selection menu.</summary>
public sealed class ChartInfo
{
    /// <summary>The chart identifier.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The chart type.</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>The type used when building "from" attribution strings.</summary>
    public string TypeForFrom { get; init; } = string.Empty;

    /// <summary>The chart title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The territory selection menu, when present.</summary>
    public ChartInfoMenu? Menu { get; init; }

    /// <summary>The chart itself, modelled as a playlist of chart positions, when present.</summary>
    public Playlist? Chart { get; init; }

    /// <summary>The chart description, when present.</summary>
    public string? ChartDescription { get; init; }
}

/// <summary>The territory menu of a <see cref="ChartInfo"/>.</summary>
public sealed class ChartInfoMenu
{
    /// <summary>The selectable territory items.</summary>
    public IReadOnlyList<ChartInfoMenuItem> Items { get; init; } = [];
}

/// <summary>A single selectable territory in a <see cref="ChartInfoMenu"/>.</summary>
public sealed class ChartInfoMenuItem
{
    /// <summary>The menu item title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The postfix used when requesting this chart, for example <c>russia</c>.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Whether this item is the currently selected territory, when known.</summary>
    public bool? Selected { get; init; }
}
