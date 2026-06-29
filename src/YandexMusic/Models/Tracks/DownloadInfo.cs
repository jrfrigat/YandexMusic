namespace YandexMusic.Models.Tracks;

/// <summary>
/// One downloadable/streamable variant of a track, returned by the download-info endpoint. Use the
/// track endpoints to resolve a variant's <see cref="DownloadInfoUrl"/> into a final, signed media URL.
/// </summary>
public sealed class DownloadInfo
{
    /// <summary>The audio codec (for example <c>mp3</c> or <c>aac</c>).</summary>
    public string Codec { get; init; } = string.Empty;

    /// <summary>The bitrate, in kbps.</summary>
    public int BitrateInKbps { get; init; }

    /// <summary>Whether replay gain information is available for this variant.</summary>
    public bool Gain { get; init; }

    /// <summary>Whether this variant is only a short preview rather than the full track.</summary>
    public bool Preview { get; init; }

    /// <summary>
    /// The URL that returns the signing parameters (host, path, ts, s) used to build the final media
    /// URL. Requires a second request; the track endpoints handle this for you.
    /// </summary>
    public string DownloadInfoUrl { get; init; } = string.Empty;

    /// <summary>Whether the media can be downloaded directly.</summary>
    public bool Direct { get; init; }
}
