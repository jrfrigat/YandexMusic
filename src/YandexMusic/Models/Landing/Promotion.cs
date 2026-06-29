using YandexMusic.Models.Tracks;

namespace YandexMusic.Models.Landing;

/// <summary>A promotional banner surfaced inside a landing block.</summary>
public sealed class Promotion
{
    /// <summary>The promotion identifier.</summary>
    public string PromoId { get; init; } = string.Empty;

    /// <summary>The promotion title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The promotion subtitle.</summary>
    public string Subtitle { get; init; } = string.Empty;

    /// <summary>The promotion heading.</summary>
    public string Heading { get; init; } = string.Empty;

    /// <summary>The web URL the promotion links to.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>The deep-link URL scheme the promotion links to.</summary>
    public string UrlScheme { get; init; } = string.Empty;

    /// <summary>The text color for rendering the banner.</summary>
    public string TextColor { get; init; } = string.Empty;

    /// <summary>The background gradient for the banner.</summary>
    public string Gradient { get; init; } = string.Empty;

    /// <summary>The banner image URI, containing a <c>%%</c> size template.</summary>
    public string Image { get; init; } = string.Empty;
}

/// <summary>A chart entry: a track together with its chart position metadata.</summary>
public sealed class ChartItem
{
    /// <summary>The track, when present.</summary>
    public Track? Track { get; init; }

    /// <summary>The chart position metadata, when present.</summary>
    public Chart? Chart { get; init; }
}

/// <summary>The chart position metadata for a track.</summary>
public sealed class Chart
{
    /// <summary>The current position in the chart.</summary>
    public int Position { get; init; }

    /// <summary>The progress indicator relative to the previous period.</summary>
    public string Progress { get; init; } = string.Empty;

    /// <summary>The number of listeners.</summary>
    public int Listeners { get; init; }

    /// <summary>The position shift relative to the previous period.</summary>
    public int Shift { get; init; }

    /// <summary>The background color for rendering, when present.</summary>
    public string? BgColor { get; init; }

    /// <summary>The referenced track id, when present.</summary>
    public TrackId? TrackId { get; init; }
}

/// <summary>A themed mix entry-point surfaced inside a landing block.</summary>
public sealed class MixLink
{
    /// <summary>The mix title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>The web URL the mix links to.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>The deep-link URL scheme the mix links to.</summary>
    public string UrlScheme { get; init; } = string.Empty;

    /// <summary>The text color for rendering the tile.</summary>
    public string TextColor { get; init; } = string.Empty;

    /// <summary>The background color for rendering the tile.</summary>
    public string BackgroundColor { get; init; } = string.Empty;

    /// <summary>The background image URI for the tile.</summary>
    public string BackgroundImageUri { get; init; } = string.Empty;

    /// <summary>The white cover URI, when present.</summary>
    public string? CoverWhite { get; init; }

    /// <summary>The cover URI, when present.</summary>
    public string? CoverUri { get; init; }
}
