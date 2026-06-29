namespace YandexMusic.Models.Tracks;

/// <summary>
/// Metadata about a track's lyrics. The actual text is hosted separately; fetch
/// <see cref="DownloadUrl"/> to retrieve it.
/// </summary>
public sealed class TrackLyrics
{
    /// <summary>The URL the lyrics text can be downloaded from.</summary>
    public string DownloadUrl { get; init; } = string.Empty;

    /// <summary>The lyrics identifier.</summary>
    public int LyricId { get; init; }

    /// <summary>The external lyrics provider identifier.</summary>
    public string ExternalLyricId { get; init; } = string.Empty;

    /// <summary>The credited writers of the lyrics, when known.</summary>
    public IReadOnlyList<string>? Writers { get; init; }

    /// <summary>The lyrics provider (major), when known.</summary>
    public LyricsMajor? Major { get; init; }
}

/// <summary>The provider (major) that supplied a track's lyrics.</summary>
public sealed class LyricsMajor
{
    /// <summary>The provider identifier.</summary>
    public int Id { get; init; }

    /// <summary>The provider name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The human-readable provider name.</summary>
    public string PrettyName { get; init; } = string.Empty;
}
